using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Registers field events, clinical interventions and the offline batches of the mobile application. The operation identifier makes a retry return the stored record instead of a second one.
/// </summary>
public class LivestockOperationCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : ILivestockOperationCommandService
{
    private const string AlreadyRegisteredNote = "The operation was already registered.";

    /// <inheritdoc />
    public async Task<Result<LivestockOperationRegistration>> Handle(RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        return await RegisterAsync(accountId, command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeMobileBatchCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (command.Operations.Count == 0 || command.Operations.Count > options.Value.MaximumBatchEntries)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.InvalidMobileBatchSize);
        }

        var results = new List<SyncEntryResult>(command.Operations.Count);
        foreach (var operation in command.Operations)
        {
            results.Add(await SynchronizeAsync(accountId, operation, cancellationToken));
        }

        return Result<IReadOnlyList<SyncEntryResult>>.Success(results);
    }

    private async Task<SyncEntryResult> SynchronizeAsync(Guid accountId, RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        var registration = await RegisterAsync(accountId, command, cancellationToken);
        if (registration.IsFailure)
        {
            var status = registration.DomainError.Type == EErrorType.Conflict ? SyncEntryStatus.Conflict : SyncEntryStatus.Rejected;
            return EntryResult(command, status, registration.DomainError.Message);
        }

        return registration.Value!.Created
            ? EntryResult(command, SyncEntryStatus.Accepted, null)
            : EntryResult(command, SyncEntryStatus.Duplicate, AlreadyRegisteredNote);
    }

    private async Task<Result<LivestockOperationRegistration>> RegisterAsync(
        Guid accountId,
        RegisterLivestockOperationCommand command,
        CancellationToken cancellationToken)
    {
        var required = RequireFields(command);
        if (required.IsFailure)
        {
            return Result<LivestockOperationRegistration>.Failure(required.DomainError);
        }

        var (animalId, type, occurredAt) = required.Value!;
        var now = clock.GetUtcNow();
        if (occurredAt > now + options.Value.ClockSkewTolerance)
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.EventTimeAhead);
        }

        var cattle = await cattleLookup.FindAsync(animalId, cancellationToken);
        if (cattle is null || (command.RanchId is { } ranchId && cattle.RanchId != ranchId))
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.AnimalNotFound);
        }

        var reproductive = ReproductiveDetails.Create(command.SireReference, command.PregnancyResult, command.ExpectedCalvingDate);
        if (reproductive.IsFailure)
        {
            return Result<LivestockOperationRegistration>.Failure(reproductive.DomainError);
        }

        var candidate = LivestockOperation.Create(command.OperationId, cattle.RanchId, cattle.CattleId, type, occurredAt,
            accountId, command.Observation, command.Product, command.Dose, now, reproductive.Value!);
        if (candidate.IsFailure)
        {
            return Result<LivestockOperationRegistration>.Failure(candidate.DomainError);
        }

        var existing = await operations.FindByIdAsync(new LivestockOperationId(command.OperationId), cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameContentAs(candidate.Value!)
                ? Result<LivestockOperationRegistration>.Success(new LivestockOperationRegistration(false, existing))
                : Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.OperationConflict);
        }

        var permission = LivestockOperation.IsClinical(type)
            ? MonitoringPermission.RecordClinicalInterventions
            : MonitoringPermission.RecordFieldEvents;
        if (!await accessControl.CanAccessRanchAsync(accountId, cattle.RanchId, permission, cancellationToken))
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        // Read after the permission check, so that an unauthorized caller learns nothing about the animal's sex.
        if (LivestockOperation.IsReproductive(type))
        {
            var profile = await cattleLookup.FindProfileAsync(cattle.RanchId, cattle.CattleId, cancellationToken);
            if (profile is null)
            {
                return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.AnimalNotFound);
            }

            if (profile.Sex != AnimalSex.Female)
            {
                return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.ReproductiveEventRequiresFemale);
            }
        }

        if (!cattle.WasActiveAt(occurredAt))
        {
            return Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.AnimalNotActive);
        }

        await operations.AddAsync(candidate.Value!, cancellationToken);
        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? Result<LivestockOperationRegistration>.Success(new LivestockOperationRegistration(true, candidate.Value!))
            : Result<LivestockOperationRegistration>.Failure(OperationsMonitoringErrors.LivestockOperationConcurrentlyStored);
    }

    private static Result<RequiredFields> RequireFields(RegisterLivestockOperationCommand command)
    {
        if (command.OperationId == Guid.Empty)
        {
            return Result<RequiredFields>.Failure(OperationsMonitoringErrors.OperationIdentifierRequired);
        }

        if (command.AnimalId is not { } animalId || animalId == Guid.Empty
            || command.Type is not { } type
            || command.OccurredAt is not { } occurredAt)
        {
            return Result<RequiredFields>.Failure(OperationsMonitoringErrors.RequiredFieldsMissing);
        }

        return Result<RequiredFields>.Success(new RequiredFields(animalId, type, MonitoringTime.Normalize(occurredAt)));
    }

    private static SyncEntryResult EntryResult(RegisterLivestockOperationCommand command, SyncEntryStatus status, string? reason) =>
        new(SyncEntryKind.LivestockOperation, command.OperationId, status, reason);

    private readonly record struct RequiredFields(Guid AnimalId, OperationType Type, DateTimeOffset OccurredAt);
}
