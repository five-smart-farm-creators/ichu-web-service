using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
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
public sealed class LivestockOperationCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations,
    IUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : ILivestockOperationCommandService
{
    /// <inheritdoc />
    public async Task<Result<LivestockOperationRegistration, ERegisterLivestockOperationError>> Handle(RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.Unauthenticated);
        }

        return await RegisterAsync(accountId, command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>> Handle(SynchronizeMobileBatchCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>.Failure(ESynchronizeMobileBatchError.Unauthenticated);
        }

        if (command.Operations.Count == 0 || command.Operations.Count > options.Value.MaximumBatchEntries)
        {
            return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>.Failure(ESynchronizeMobileBatchError.InvalidBatchSize);
        }

        var results = new List<SyncEntryResult>(command.Operations.Count);
        foreach (var operation in command.Operations)
        {
            results.Add(await SynchronizeAsync(accountId, operation, cancellationToken));
        }

        return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeMobileBatchError>.Success(results);
    }

    private static SyncEntryResult EntryResult(RegisterLivestockOperationCommand command, SyncEntryStatus status, string? reason) =>
        new(SyncEntryKind.LivestockOperation, command.OperationId, status, reason);

    private static SyncEntryResult ToEntryResult(
        RegisterLivestockOperationCommand command,
        Result<LivestockOperationRegistration, ERegisterLivestockOperationError> result) =>
        result switch
        {
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success { Value.Created: true } =>
                EntryResult(command, SyncEntryStatus.Accepted, null),
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success =>
                EntryResult(command, SyncEntryStatus.Duplicate, "The operation was already registered."),
            Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure failure => failure.Error switch
            {
                ERegisterLivestockOperationError.OperationConflict =>
                    EntryResult(command, SyncEntryStatus.Conflict, "A different operation already uses this identifier."),
                ERegisterLivestockOperationError.AnimalNotActive =>
                    EntryResult(command, SyncEntryStatus.Conflict, "The animal was not active in the herd when the operation occurred."),
                ERegisterLivestockOperationError.AnimalNotFound =>
                    EntryResult(command, SyncEntryStatus.Rejected, "The animal does not exist."),
                ERegisterLivestockOperationError.EventTimeAhead =>
                    EntryResult(command, SyncEntryStatus.Rejected, "The event time is ahead of the server clock beyond the configured tolerance."),
                _ => EntryResult(command, SyncEntryStatus.Rejected, "The account is not authorized for this ranch operation.")
            },
            _ => throw new InvalidOperationException("Unknown result type.")
        };

    private async Task<SyncEntryResult> SynchronizeAsync(Guid accountId, RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return ToEntryResult(command, await RegisterAsync(accountId, command, cancellationToken));
        }
        catch (ArgumentException ex)
        {
            return EntryResult(command, SyncEntryStatus.Rejected, ex.Message);
        }
        catch (DbUpdateException)
        {
            unitOfWork.DiscardChanges();
            return EntryResult(command, SyncEntryStatus.Conflict, "The operation changed concurrently or already exists; send it again if it was not stored.");
        }
    }

    private async Task<Result<LivestockOperationRegistration, ERegisterLivestockOperationError>> RegisterAsync(
        Guid accountId,
        RegisterLivestockOperationCommand command,
        CancellationToken cancellationToken)
    {
        var (animalId, type, occurredAt) = RequireFields(command);
        var now = clock.GetUtcNow();
        if (occurredAt > now + options.Value.ClockSkewTolerance)
        {
            return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.EventTimeAhead);
        }

        var cattle = await cattleLookup.FindAsync(animalId, cancellationToken);
        if (cattle is null)
        {
            return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.AnimalNotFound);
        }

        var candidate = new LivestockOperation(command.OperationId, cattle.RanchId, cattle.CattleId, type, occurredAt,
            accountId, command.Observation, command.Product, command.Dose, now);

        var existing = await operations.FindByIdAsync(command.OperationId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameContentAs(candidate)
                ? new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success(new LivestockOperationRegistration(false, existing))
                : new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.OperationConflict);
        }

        var permission = LivestockOperation.IsClinical(type)
            ? MonitoringPermission.RecordClinicalInterventions
            : MonitoringPermission.RecordFieldEvents;
        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, permission, cancellationToken))
        {
            return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.Forbidden);
        }

        if (!cattle.WasActiveAt(occurredAt))
        {
            return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Failure(ERegisterLivestockOperationError.AnimalNotActive);
        }

        await operations.AddAsync(candidate, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return new Result<LivestockOperationRegistration, ERegisterLivestockOperationError>.Success(new LivestockOperationRegistration(true, candidate));
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

        return (command.AnimalId!.Value, command.Type!.Value, command.OccurredAt!.Value.ToUniversalTime());
    }
}
