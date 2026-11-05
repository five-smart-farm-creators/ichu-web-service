using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>Registers field events (US-21), clinical interventions (US-23) and offline mobile batches (US-22, TS-18).</summary>
public sealed class LivestockOperationService(
    MonitoringAuthorization authorization,
    SyncEntryProcessor entryProcessor,
    ICattleLookup cattleLookup,
    ILivestockOperationRepository operations,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock)
{
    public async Task<LivestockOperationResult> RegisterFieldEventAsync(RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        return await RegisterAsync(accountId, command, requireClinical: false, cancellationToken);
    }

    public async Task<LivestockOperationResult> RegisterClinicalInterventionAsync(RegisterLivestockOperationCommand command, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        return await RegisterAsync(accountId, command, requireClinical: true, cancellationToken);
    }

    public async Task<IReadOnlyList<SyncEntryResult>> SynchronizeMobileBatchAsync(
        IReadOnlyList<RegisterLivestockOperationCommand> commands,
        CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        var maximum = options.Value.MaximumBatchEntries;
        if (commands.Count == 0 || commands.Count > maximum)
        {
            throw MonitoringException.InvalidInput($"A batch must contain between 1 and {maximum} operations.");
        }

        var results = new List<SyncEntryResult>(commands.Count);
        foreach (var command in commands)
        {
            results.Add(await entryProcessor.ProcessAsync(SyncEntryKind.LivestockOperation, command.OperationId, async () =>
            {
                var result = await RegisterAsync(accountId, command, requireClinical: null, cancellationToken);
                return new SyncEntryResult(SyncEntryKind.LivestockOperation, command.OperationId, result.Status, result.Reason);
            }));
        }

        return results;
    }

    public async Task<IReadOnlyList<LivestockOperation>> GetHistoryAsync(GetLivestockOperationsQuery query, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        var cattle = await cattleLookup.FindAsync(query.AnimalId, cancellationToken)
            ?? throw MonitoringException.NotFound("The animal does not exist.");
        await authorization.EnsureAsync(accountId, cattle.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken);
        return await operations.FindByCattleAsync(query.AnimalId, query.Period, cancellationToken);
    }

    private async Task<LivestockOperationResult> RegisterAsync(
        Guid accountId,
        RegisterLivestockOperationCommand command,
        bool? requireClinical,
        CancellationToken cancellationToken)
    {
        var (animalId, type, occurredAt) = ValidateRequiredFields(command);
        if (requireClinical is { } clinical && LivestockOperation.IsClinical(type) != clinical)
        {
            throw MonitoringException.InvalidInput(clinical
                ? "Clinical interventions are Vaccination, Treatment or Insemination."
                : "Field events are HealthEvent or Birth.");
        }

        var now = clock.GetUtcNow();
        if (occurredAt > now + options.Value.ClockSkewTolerance)
        {
            throw MonitoringException.InvalidInput("The event time is ahead of the server clock beyond the configured tolerance.");
        }

        var cattle = await cattleLookup.FindAsync(animalId, cancellationToken)
            ?? throw MonitoringException.NotFound("The animal does not exist.");
        var candidate = new LivestockOperation(command.OperationId, cattle.RanchId, cattle.CattleId, type, occurredAt,
            accountId, command.Observation, command.Product, command.Dose, now);

        var existing = await operations.FindAsync(command.OperationId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameContentAs(candidate)
                ? new LivestockOperationResult(SyncEntryStatus.Duplicate, existing, "The operation was already registered.")
                : new LivestockOperationResult(SyncEntryStatus.Conflict, null, "A different operation already uses this identifier.");
        }

        var permission = LivestockOperation.IsClinical(type)
            ? MonitoringPermission.RecordClinicalInterventions
            : MonitoringPermission.RecordFieldEvents;
        await authorization.EnsureAsync(accountId, cattle.RanchId, permission, cancellationToken);

        if (!cattle.WasActiveAt(occurredAt))
        {
            throw MonitoringException.Conflict("The animal was not active in the herd when the operation occurred.");
        }

        operations.Add(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new LivestockOperationResult(SyncEntryStatus.Accepted, candidate, null);
    }

    private static (Guid AnimalId, OperationType Type, DateTimeOffset OccurredAt) ValidateRequiredFields(RegisterLivestockOperationCommand command)
    {
        if (command.OperationId == Guid.Empty)
        {
            throw MonitoringException.InvalidInput("An operation identifier is required.");
        }

        var missing = new List<string>();
        if (command.AnimalId is null || command.AnimalId == Guid.Empty)
        {
            missing.Add("animalId");
        }

        if (string.IsNullOrWhiteSpace(command.Type))
        {
            missing.Add("type");
        }

        if (command.OccurredAt is null)
        {
            missing.Add("occurredAt");
        }

        if (missing.Count > 0)
        {
            throw MonitoringException.InvalidInput($"Missing required fields: {string.Join(", ", missing)}.");
        }

        var type = MonitoringValueParser.ParseEnum<OperationType>(command.Type, "operation type");
        return (command.AnimalId!.Value, type, command.OccurredAt!.Value.ToUniversalTime());
    }
}
