using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Application command service that implements <see cref="ILivestockOperationCommandService"/> to register field
///     events, clinical interventions and the offline batches of the mobile application.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="operations">The livestock operation repository.</param>
/// <param name="unitOfWork">The unit of work used to commit changes.</param>
/// <param name="options">The technical limits of the module.</param>
/// <param name="clock">The clock that provides the server time.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
public class LivestockOperationCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations,
    IUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock,
    ILogger<LivestockOperationCommandService> logger) : ILivestockOperationCommandService
{
    /// <inheritdoc />
    public async Task<Result<LivestockOperationRegistration>> Handle(RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        return await RegisterAsync(accountId, command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeMobileBatchCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        if (command.Operations.Count == 0 || command.Operations.Count > options.Value.MaximumBatchEntries)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.InvalidMobileBatchSize, OperationsMonitoringError.InvalidMobileBatchSize);
        }

        var results = new List<SyncEntryResult>(command.Operations.Count);
        foreach (var operation in command.Operations)
        {
            results.Add(await SynchronizeAsync(accountId, operation, cancellationToken));
        }

        return Result<IReadOnlyList<SyncEntryResult>>.Success(results);
    }

    private static SyncEntryResult EntryResult(RegisterLivestockOperationCommand command, SyncEntryStatus status, string? reason) =>
        new(SyncEntryKind.LivestockOperation, command.OperationId, status, reason);

    private static SyncEntryResult ToEntryResult(RegisterLivestockOperationCommand command, Result<LivestockOperationRegistration> result)
    {
        if (result.IsSuccess)
        {
            return result.Value!.Created
                ? EntryResult(command, SyncEntryStatus.Accepted, null)
                : EntryResult(command, SyncEntryStatus.Duplicate, "The operation was already registered.");
        }

        var status = result.Error is OperationsMonitoringError.OperationConflict or OperationsMonitoringError.AnimalNotActive
            ? SyncEntryStatus.Conflict
            : SyncEntryStatus.Rejected;
        return EntryResult(command, status, result.Message);
    }

    private async Task<SyncEntryResult> SynchronizeAsync(Guid accountId, RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return ToEntryResult(command, await RegisterAsync(accountId, command, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Rejected a synchronization entry: {Reason}", ex.Message);
            return EntryResult(command, SyncEntryStatus.Rejected, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "The changes could not be saved.");
            unitOfWork.DiscardChanges();
            return EntryResult(command, SyncEntryStatus.Conflict, "The operation changed concurrently or already exists; send it again if it was not stored.");
        }
    }

    private async Task<Result<LivestockOperationRegistration>> RegisterAsync(
        Guid accountId,
        RegisterLivestockOperationCommand command,
        CancellationToken cancellationToken)
    {
        var (animalId, type, occurredAt) = RequireFields(command);
        var now = clock.GetUtcNow();
        if (occurredAt > now + options.Value.ClockSkewTolerance)
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.EventTimeAhead, OperationsMonitoringError.EventTimeAhead);
        }

        var cattle = await cattleLookup.FindAsync(animalId, cancellationToken);
        if (cattle is null)
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.AnimalNotFound, OperationsMonitoringError.AnimalNotFound);
        }

        var candidate = new LivestockOperation(command.OperationId, cattle.RanchId, cattle.CattleId, type, occurredAt,
            accountId, command.Observation, command.Product, command.Dose, now);

        var existing = await operations.FindByIdAsync(command.OperationId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameContentAs(candidate)
                ? Result<LivestockOperationRegistration>.Success(new LivestockOperationRegistration(false, existing))
                : Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.OperationConflict, OperationsMonitoringError.OperationConflict);
        }

        var permission = LivestockOperation.IsClinical(type)
            ? MonitoringPermission.RecordClinicalInterventions
            : MonitoringPermission.RecordFieldEvents;
        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, permission, cancellationToken))
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
        }

        if (!cattle.WasActiveAt(occurredAt))
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.AnimalNotActive, OperationsMonitoringError.AnimalNotActive);
        }

        await operations.AddAsync(candidate, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<LivestockOperationRegistration>.Success(new LivestockOperationRegistration(true, candidate));
    }

    private static (Guid AnimalId, OperationType Type, DateTimeOffset OccurredAt) RequireFields(RegisterLivestockOperationCommand command)
    {
        if (command.OperationId == Guid.Empty)
        {
            throw new ArgumentException("An operation identifier is required.");
        }

        var missing = new List<string>();
        if (command.AnimalId is null || command.AnimalId == Guid.Empty)
        {
            missing.Add("animalId");
        }

        if (command.Type is null)
        {
            missing.Add("type");
        }

        if (command.OccurredAt is null)
        {
            missing.Add("occurredAt");
        }

        if (missing.Count > 0)
        {
            throw new ArgumentException($"Missing or invalid required fields: {string.Join(", ", missing)}.");
        }

        return (command.AnimalId!.Value, command.Type!.Value, MonitoringTime.Normalize(command.OccurredAt!.Value));
    }
}
