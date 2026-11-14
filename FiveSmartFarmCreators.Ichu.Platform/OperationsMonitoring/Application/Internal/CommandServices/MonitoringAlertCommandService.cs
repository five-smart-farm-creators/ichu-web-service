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
///     Attends alerts and synchronizes the alerts and alert changes sent by Edge Gateways. Each entry is decided on its own, so a rejected entry never blocks the others.
/// </summary>
public class MonitoringAlertCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IMonitoringAlertRepository alerts,
    IGeofenceRepository geofences,
    IMonitoringRuleSetRepository ruleSets,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IMonitoringAlertCommandService
{
    private const string AlreadyJoinedNote = "The alert already joined an open alert of the same condition.";
    private const string JoinedNote = "The alert joined the open alert of the same condition.";
    private const string AlreadySynchronizedNote = "The alert was already synchronized.";
    private const string AlreadyAppliedNote = "The change was already applied.";

    /// <inheritdoc />
    public async Task<Result<MonitoringAlert>> Handle(AcknowledgeAlertCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.AttendAlerts, cancellationToken))
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        // The ranch is checked before the lookup, and an alert of another ranch is reported as missing, so identifiers cannot be probed.
        var alert = await alerts.FindByIdAsync(command.AlertId, cancellationToken);
        if (alert is null || alert.RanchId != command.RanchId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertNotFound);
        }

        var now = clock.GetUtcNow();
        var outcome = Acknowledge(alert, command, accountId, now);
        if (outcome.IsFailure)
        {
            return Result<MonitoringAlert>.Failure(outcome.DomainError);
        }

        if (outcome.Value == AlertChangeOutcome.Conflict)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertConflict);
        }

        if (outcome.Value == AlertChangeOutcome.AlreadyApplied)
        {
            return Result<MonitoringAlert>.Success(alert);
        }

        if (await unitOfWork.TryCompleteAsync(cancellationToken))
        {
            return Result<MonitoringAlert>.Success(alert);
        }

        // Only a replay of the same operation can be confirmed; any other concurrent change is a conflict.
        var current = await alerts.FindByIdAsync(command.AlertId, cancellationToken);
        return current is not null && current.HasApplied(command.OperationId) && IsReplay(current, command, accountId, now)
            ? Result<MonitoringAlert>.Success(current)
            : Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertConflict);
    }

    /// <inheritdoc />
    public Task<SyncEntryResult> Handle(SynchronizeAlertCommand command, CancellationToken cancellationToken) =>
        SynchronizeAlertAsync(command, cancellationToken);

    /// <inheritdoc />
    public Task<SyncEntryResult> Handle(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken) =>
        SynchronizeAlertChangeAsync(command, cancellationToken);

    private static Result<AlertChangeOutcome> Acknowledge(MonitoringAlert alert, AcknowledgeAlertCommand command, Guid accountId, DateTimeOffset now) =>
        alert.Acknowledge(command.OperationId, accountId, command.Finding, command.Action, now, command.ExpectedVersion, now);

    private static bool IsReplay(MonitoringAlert alert, AcknowledgeAlertCommand command, Guid accountId, DateTimeOffset now) =>
        Acknowledge(alert, command, accountId, now) is { IsSuccess: true, Value: AlertChangeOutcome.AlreadyApplied };

    private async Task<SyncEntryResult> SynchronizeAlertAsync(SynchronizeAlertCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.Type is not { } type)
        {
            return AlertRejected(entry, OperationsMonitoringErrors.AlertTypeUnknown);
        }

        if (entry.Severity is not { } severity)
        {
            return AlertRejected(entry, OperationsMonitoringErrors.AlertSeverityUnknown);
        }

        var supporting = await LoadReadingsAsync(entry.DeviceId, entry.ReadingIds, cancellationToken);
        if (supporting.IsFailure)
        {
            return AlertRejected(entry, supporting.DomainError);
        }

        var evidence = supporting.Value!;
        if (evidence[0].RanchId != command.RanchId)
        {
            return AlertRejected(entry, OperationsMonitoringErrors.AlertGatewayAccessDenied);
        }

        // An alert that previously joined an open alert is recorded there as evidence under its own identifier.
        if (await alerts.FindByOperationIdAsync(entry.AlertId.Value, cancellationToken) is { } absorbing)
        {
            var joined = absorbing.AddEvidence(entry.AlertId.Value, evidence, command.ReceivedAt);
            if (joined.IsFailure)
            {
                return AlertRejected(entry, joined.DomainError);
            }

            return joined.Value == AlertChangeOutcome.AlreadyApplied
                ? AlertResult(entry, SyncEntryStatus.Duplicate, AlreadyJoinedNote, absorbing.Id)
                : AlertConflict(entry, OperationsMonitoringErrors.AlertIdentifierConflict, absorbing.Id);
        }

        var candidate = await RaiseAlertAsync(command, type, severity, evidence, cancellationToken);
        if (candidate.IsFailure)
        {
            return AlertRejected(entry, candidate.DomainError);
        }

        var raised = candidate.Value!;
        var existing = await alerts.FindByIdAsNoTrackingAsync(entry.AlertId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameOriginAs(raised)
                ? AlertResult(entry, SyncEntryStatus.Duplicate, AlreadySynchronizedNote)
                : AlertConflict(entry, OperationsMonitoringErrors.AlertIdentifierConflict);
        }

        var cattle = await cattleLookup.FindAsync(raised.CattleId, cancellationToken);
        if (cattle is null || cattle.RanchId != command.RanchId || !cattle.WasActiveAt(raised.RaisedAt))
        {
            return AlertRejected(entry, OperationsMonitoringErrors.AlertAnimalNotActive);
        }

        var pending = await alerts.FindPendingByCattleAndTypeAsync(raised.CattleId, type, cancellationToken);
        var open = pending.FirstOrDefault(a => a.IsOpenDuplicateCandidate(type, raised.Severity, raised.CattleId, raised.RaisedAt));
        if (open is not null)
        {
            var joined = open.AddEvidence(entry.AlertId.Value, evidence, command.ReceivedAt);
            if (joined.IsFailure)
            {
                return AlertRejected(entry, joined.DomainError);
            }

            return await unitOfWork.TryCompleteAsync(cancellationToken)
                ? AlertResult(entry, SyncEntryStatus.Duplicate, JoinedNote, open.Id)
                : AlertConflict(entry, OperationsMonitoringErrors.AlertConcurrentlyStored);
        }

        await alerts.AddAsync(raised, cancellationToken);
        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? AlertResult(entry, SyncEntryStatus.Accepted, null)
            : AlertConflict(entry, OperationsMonitoringErrors.AlertConcurrentlyStored);
    }

    private async Task<Result<MonitoringAlert>> RaiseAlertAsync(
        SynchronizeAlertCommand command,
        AlertType type,
        AlertSeverity severity,
        IReadOnlyList<TelemetryReading> evidence,
        CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        var ruleSet = await ruleSets.FindByRanchAsNoTrackingAsync(command.RanchId, cancellationToken);
        if (ruleSet is null || entry.ConfigurationRevision > ruleSet.Revision)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertRevisionNotPublished);
        }

        if (type != AlertType.Geofence)
        {
            if (entry.ThresholdMinValue is not { } min || entry.ThresholdMaxValue is not { } max)
            {
                return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.ThresholdAlertRequiresThreshold);
            }

            var threshold = ThresholdRange.Create(min, max);
            return threshold.IsFailure
                ? Result<MonitoringAlert>.Failure(threshold.DomainError)
                : MonitoringAlert.RaiseForThreshold(entry.AlertId, type, severity, threshold.Value!, entry.ConfigurationRevision,
                    command.GatewayId, evidence, command.ReceivedAt);
        }

        if (entry.GeofenceId is not { } geofenceId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.GeofenceAlertRequiresGeofence);
        }

        var geofence = await geofences.FindByIdAsync(geofenceId, cancellationToken);
        if (geofence is null || geofence.RanchId != command.RanchId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.GeofenceNotInRanch);
        }

        if (entry.DistanceToBoundaryMeters is not { } distance)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.GeofenceAlertDistanceRequired);
        }

        return MonitoringAlert.RaiseForGeofence(entry.AlertId, severity, geofenceId, distance, entry.ConfigurationRevision,
            command.GatewayId, evidence, command.ReceivedAt);
    }

    private async Task<SyncEntryResult> SynchronizeAlertChangeAsync(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.Kind is not { } kind)
        {
            return ChangeRejected(entry, OperationsMonitoringErrors.ChangeKindUnknown);
        }

        var alert = await alerts.FindByIdAsync(entry.AlertId, cancellationToken);
        if (alert is null)
        {
            return ChangeRejected(entry, OperationsMonitoringErrors.AlertNotSynchronized);
        }

        if (alert.RanchId != command.RanchId)
        {
            return ChangeRejected(entry, OperationsMonitoringErrors.AlertChangeGatewayAccessDenied);
        }

        var outcome = await ApplyChangeAsync(alert, kind, command, cancellationToken);
        if (outcome.IsFailure)
        {
            return ChangeRejected(entry, outcome.DomainError);
        }

        if (outcome.Value == AlertChangeOutcome.AlreadyApplied)
        {
            return ChangeResult(entry, SyncEntryStatus.Duplicate, AlreadyAppliedNote, alert.Id);
        }

        if (outcome.Value == AlertChangeOutcome.Conflict)
        {
            return ChangeConflict(entry, OperationsMonitoringErrors.AlertChangeConflict, alert.Id);
        }

        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? ChangeResult(entry, SyncEntryStatus.Accepted, null, alert.Id)
            : ChangeConflict(entry, OperationsMonitoringErrors.AlertChangeConcurrentlyStored, alert.Id);
    }

    private async Task<Result<AlertChangeOutcome>> ApplyChangeAsync(
        MonitoringAlert alert,
        AlertChangeKind kind,
        SynchronizeAlertChangeCommand command,
        CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (kind == AlertChangeKind.Evidence)
        {
            var evidence = await LoadReadingsAsync(entry.DeviceId ?? alert.DeviceId, entry.ReadingIds, cancellationToken);
            return evidence.IsFailure
                ? Result<AlertChangeOutcome>.Failure(evidence.DomainError)
                : alert.AddEvidence(entry.OperationId, evidence.Value!, command.ReceivedAt);
        }

        var eventTime = RequireEventTime(entry.OccurredAt, command.ReceivedAt);
        if (eventTime.IsFailure)
        {
            return Result<AlertChangeOutcome>.Failure(eventTime.DomainError);
        }

        if (kind == AlertChangeKind.Cleared)
        {
            return alert.Clear(entry.OperationId, eventTime.Value!, command.ReceivedAt);
        }

        return await AcknowledgeFromEdgeAsync(alert, entry, eventTime.Value!, command.ReceivedAt, cancellationToken);
    }

    private async Task<Result<AlertChangeOutcome>> AcknowledgeFromEdgeAsync(
        MonitoringAlert alert,
        EdgeAlertChangeEntry entry,
        DateTimeOffset acknowledgedAt,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        if (entry.AuthorId is not { } authorId)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.EdgeAttentionAuthorRequired);
        }

        if (entry.ExpectedVersion is not { } expectedVersion)
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.EdgeAttentionVersionRequired);
        }

        // A retried operation keeps its original result even if the author's access changed afterwards.
        if (!alert.HasApplied(entry.OperationId)
            && !await accessControl.CanAccessRanchAsync(authorId, alert.RanchId, MonitoringPermission.AttendAlerts, cancellationToken))
        {
            return Result<AlertChangeOutcome>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        return alert.Acknowledge(entry.OperationId, authorId, entry.Finding ?? string.Empty, entry.Action,
            acknowledgedAt, expectedVersion, receivedAt);
    }

    private async Task<Result<IReadOnlyList<TelemetryReading>>> LoadReadingsAsync(
        Guid deviceId,
        IReadOnlyList<Guid> readingIds,
        CancellationToken cancellationToken)
    {
        if (readingIds.Count == 0)
        {
            return Result<IReadOnlyList<TelemetryReading>>.Failure(OperationsMonitoringErrors.SupportingReadingRequired);
        }

        if (readingIds.Distinct().Count() != readingIds.Count)
        {
            return Result<IReadOnlyList<TelemetryReading>>.Failure(OperationsMonitoringErrors.SupportingReadingsRepeated);
        }

        var found = (await readings.FindByDeviceAndReadingIdsAsync(deviceId, readingIds, cancellationToken))
            .ToDictionary(reading => reading.ReadingId);
        var ordered = new List<TelemetryReading>(readingIds.Count);
        foreach (var readingId in readingIds)
        {
            if (!found.TryGetValue(readingId, out var reading))
            {
                return Result<IReadOnlyList<TelemetryReading>>.Failure(OperationsMonitoringErrors.SupportingReadingNotSynchronized);
            }

            ordered.Add(reading);
        }

        return Result<IReadOnlyList<TelemetryReading>>.Success(ordered);
    }

    private Result<DateTimeOffset> RequireEventTime(DateTimeOffset? occurredAt, DateTimeOffset receivedAt)
    {
        if (occurredAt is not { } eventTime)
        {
            return Result<DateTimeOffset>.Failure(OperationsMonitoringErrors.EventTimeRequired);
        }

        var instant = MonitoringTime.Normalize(eventTime);
        return instant > receivedAt + options.Value.ClockSkewTolerance
            ? Result<DateTimeOffset>.Failure(OperationsMonitoringErrors.EventTimeAhead)
            : Result<DateTimeOffset>.Success(instant);
    }

    private static SyncEntryResult AlertRejected(EdgeAlertEntry entry, Error error) =>
        AlertResult(entry, SyncEntryStatus.Rejected, error.Message);

    private static SyncEntryResult AlertConflict(EdgeAlertEntry entry, Error error, MonitoringAlertId? relatedAlertId = null) =>
        AlertResult(entry, SyncEntryStatus.Conflict, error.Message, relatedAlertId);

    private static SyncEntryResult AlertResult(EdgeAlertEntry entry, SyncEntryStatus status, string? reason, MonitoringAlertId? relatedAlertId = null) =>
        new(SyncEntryKind.Alert, entry.AlertId.Value, status, reason, relatedAlertId);

    private static SyncEntryResult ChangeRejected(EdgeAlertChangeEntry entry, Error error) =>
        ChangeResult(entry, SyncEntryStatus.Rejected, error.Message);

    private static SyncEntryResult ChangeConflict(EdgeAlertChangeEntry entry, Error error, MonitoringAlertId? relatedAlertId = null) =>
        ChangeResult(entry, SyncEntryStatus.Conflict, error.Message, relatedAlertId);

    private static SyncEntryResult ChangeResult(EdgeAlertChangeEntry entry, SyncEntryStatus status, string? reason, MonitoringAlertId? relatedAlertId = null) =>
        new(SyncEntryKind.AlertChange, entry.OperationId, status, reason, relatedAlertId);
}
