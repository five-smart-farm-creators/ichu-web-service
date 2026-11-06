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
///     Application command service that implements <see cref="IMonitoringAlertCommandService"/> to attend alerts and
///     to synchronize the alerts and alert changes sent by Edge Gateways.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="cattleLookup">The anti-corruption port towards Cattle Information.</param>
/// <param name="readings">The telemetry reading repository.</param>
/// <param name="alerts">The monitoring alert repository.</param>
/// <param name="geofences">The geofence repository.</param>
/// <param name="ruleSets">The monitoring rule set repository.</param>
/// <param name="unitOfWork">The unit of work used to commit changes.</param>
/// <param name="options">The technical limits of the module.</param>
/// <param name="clock">The clock that provides the server time.</param>
public sealed class MonitoringAlertCommandService(
    IMonitoringAccessControl accessControl,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IMonitoringAlertRepository alerts,
    IGeofenceRepository geofences,
    IMonitoringRuleSetRepository ruleSets,
    IUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IMonitoringAlertCommandService
{
    /// <inheritdoc />
    public async Task<Result<MonitoringAlert, EAcknowledgeAlertError>> Handle(AcknowledgeAlertCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Failure(EAcknowledgeAlertError.Unauthenticated);
        }

        var alert = await alerts.FindByIdAsync(command.AlertId, cancellationToken);
        if (alert is null)
        {
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Failure(EAcknowledgeAlertError.AlertNotFound);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, alert.RanchId, MonitoringPermission.AttendAlerts, cancellationToken))
        {
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Failure(EAcknowledgeAlertError.Forbidden);
        }

        var now = clock.GetUtcNow();
        var outcome = Acknowledge(alert, command, accountId, now);
        if (outcome == AlertChangeOutcome.Conflict)
        {
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Failure(EAcknowledgeAlertError.Conflict);
        }

        if (outcome == AlertChangeOutcome.AlreadyApplied)
        {
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Success(alert);
        }

        try
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<MonitoringAlert, EAcknowledgeAlertError>.Success(alert);
        }
        catch (DbUpdateException)
        {
            unitOfWork.DiscardChanges();
            var current = await alerts.FindByIdAsync(command.AlertId, cancellationToken);

            // Only a replay of the same operation can be confirmed; any other concurrent change is a conflict.
            return current is not null
                   && current.HasApplied(command.OperationId)
                   && Acknowledge(current, command, accountId, now) == AlertChangeOutcome.AlreadyApplied
                ? new Result<MonitoringAlert, EAcknowledgeAlertError>.Success(current)
                : new Result<MonitoringAlert, EAcknowledgeAlertError>.Failure(EAcknowledgeAlertError.Conflict);
        }
    }

    /// <inheritdoc />
    public async Task<SyncEntryResult> Handle(SynchronizeAlertCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await SynchronizeAlertAsync(command, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return AlertResult(command.Entry, SyncEntryStatus.Rejected, ex.Message);
        }
        catch (DbUpdateException)
        {
            unitOfWork.DiscardChanges();
            return AlertResult(command.Entry, SyncEntryStatus.Conflict, "The alert changed concurrently or already exists; send it again if it was not stored.");
        }
    }

    /// <inheritdoc />
    public async Task<SyncEntryResult> Handle(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            return await SynchronizeAlertChangeAsync(command, cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return ChangeResult(command.Entry, SyncEntryStatus.Rejected, ex.Message);
        }
        catch (DbUpdateException)
        {
            unitOfWork.DiscardChanges();
            return ChangeResult(command.Entry, SyncEntryStatus.Conflict, "The alert changed concurrently; send the change again if it was not applied.");
        }
    }

    private static AlertChangeOutcome Acknowledge(MonitoringAlert alert, AcknowledgeAlertCommand command, Guid accountId, DateTimeOffset now) =>
        alert.Acknowledge(command.OperationId, accountId, command.Finding, command.Action, now, command.ExpectedVersion, now);

    private static SyncEntryResult AlertResult(EdgeAlertEntry entry, SyncEntryStatus status, string? reason, Guid? relatedAlertId = null) =>
        new(SyncEntryKind.Alert, entry.AlertId, status, reason, relatedAlertId);

    private static SyncEntryResult ChangeResult(EdgeAlertChangeEntry entry, SyncEntryStatus status, string? reason, Guid? relatedAlertId = null) =>
        new(SyncEntryKind.AlertChange, entry.OperationId, status, reason, relatedAlertId);

    private static string NamesOf<TEnum>() where TEnum : struct, Enum => string.Join(", ", Enum.GetNames<TEnum>());

    private async Task<SyncEntryResult> SynchronizeAlertAsync(SynchronizeAlertCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.Type is not { } type)
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, $"The alert type must be one of: {NamesOf<AlertType>()}.");
        }

        if (entry.Severity is not { } severity)
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, $"The alert severity must be one of: {NamesOf<AlertSeverity>()}.");
        }

        var (supporting, loadError) = await LoadReadingsAsync(entry.DeviceId, entry.ReadingIds, cancellationToken);
        if (loadError is not null)
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, loadError);
        }

        if (supporting[0].RanchId != command.RanchId)
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, "The gateway has no access to the ranch of this alert.");
        }

        // An alert that previously joined an open alert is recorded there as evidence under its own identifier.
        if (await alerts.FindByOperationIdAsync(entry.AlertId, cancellationToken) is { } absorbing)
        {
            return absorbing.AddEvidence(entry.AlertId, supporting, command.ReceivedAt) == AlertChangeOutcome.AlreadyApplied
                ? AlertResult(entry, SyncEntryStatus.Duplicate, "The alert already joined an open alert of the same condition.", absorbing.Id)
                : AlertResult(entry, SyncEntryStatus.Conflict, "A different alert already uses this identifier.", absorbing.Id);
        }

        var raised = await RaiseAlertAsync(command, type, severity, supporting, cancellationToken);
        if (raised is Result<MonitoringAlert, string>.Failure rejection)
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, rejection.Error);
        }

        var candidate = ((Result<MonitoringAlert, string>.Success)raised).Value;
        var existing = await alerts.FindByIdAsync(entry.AlertId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameOriginAs(candidate)
                ? AlertResult(entry, SyncEntryStatus.Duplicate, "The alert was already synchronized.")
                : AlertResult(entry, SyncEntryStatus.Conflict, "A different alert already uses this identifier.");
        }

        var cattle = await cattleLookup.FindAsync(candidate.CattleId, cancellationToken);
        if (cattle is null || cattle.RanchId != command.RanchId || !cattle.WasActiveAt(candidate.RaisedAt))
        {
            return AlertResult(entry, SyncEntryStatus.Rejected, "An animal that is not active in the herd does not generate alerts.");
        }

        var pending = await alerts.FindPendingByCattleAndTypeAsync(candidate.CattleId, type, cancellationToken);
        var open = pending.FirstOrDefault(a => a.IsOpenDuplicateCandidate(type, candidate.CattleId, candidate.RaisedAt));
        if (open is not null)
        {
            open.AddEvidence(entry.AlertId, supporting, command.ReceivedAt);
            await unitOfWork.CompleteAsync(cancellationToken);
            return AlertResult(entry, SyncEntryStatus.Duplicate, "The alert joined the open alert of the same condition.", open.Id);
        }

        await alerts.AddAsync(candidate, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return AlertResult(entry, SyncEntryStatus.Accepted, null);
    }

    private async Task<Result<MonitoringAlert, string>> RaiseAlertAsync(
        SynchronizeAlertCommand command,
        AlertType type,
        AlertSeverity severity,
        IReadOnlyList<TelemetryReading> supporting,
        CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        var ruleSet = await ruleSets.FindByRanchAsync(command.RanchId, cancellationToken);
        if (ruleSet is null || entry.ConfigurationRevision > ruleSet.Revision)
        {
            return new Result<MonitoringAlert, string>.Failure("The alert references a configuration revision that was never published.");
        }

        if (type != AlertType.Geofence)
        {
            if (entry.ThresholdMinValue is not { } min || entry.ThresholdMaxValue is not { } max)
            {
                return new Result<MonitoringAlert, string>.Failure("A threshold alert requires the threshold used to evaluate it.");
            }

            return new Result<MonitoringAlert, string>.Success(MonitoringAlert.RaiseForThreshold(entry.AlertId, type, severity,
                new ThresholdRange(min, max), entry.ConfigurationRevision, command.GatewayId, supporting, command.ReceivedAt));
        }

        if (entry.GeofenceId is not { } geofenceId)
        {
            return new Result<MonitoringAlert, string>.Failure("A geofence alert requires its geofence.");
        }

        var geofence = await geofences.FindByIdAsync(geofenceId, cancellationToken);
        if (geofence is null || geofence.RanchId != command.RanchId)
        {
            return new Result<MonitoringAlert, string>.Failure("The geofence does not exist for this ranch.");
        }

        if (entry.DistanceToBoundaryMeters is not { } distance)
        {
            return new Result<MonitoringAlert, string>.Failure("A geofence alert requires the distance to the boundary.");
        }

        return new Result<MonitoringAlert, string>.Success(MonitoringAlert.RaiseForGeofence(entry.AlertId, severity, geofenceId, distance,
            entry.ConfigurationRevision, command.GatewayId, supporting, command.ReceivedAt));
    }

    private async Task<SyncEntryResult> SynchronizeAlertChangeAsync(SynchronizeAlertChangeCommand command, CancellationToken cancellationToken)
    {
        var entry = command.Entry;
        if (entry.Kind is not { } kind)
        {
            return ChangeResult(entry, SyncEntryStatus.Rejected, $"The change kind must be one of: {NamesOf<AlertChangeKind>()}.");
        }

        var alert = await alerts.FindByIdAsync(entry.AlertId, cancellationToken);
        if (alert is null)
        {
            return ChangeResult(entry, SyncEntryStatus.Rejected, "The alert has not been synchronized.");
        }

        if (alert.RanchId != command.RanchId)
        {
            return ChangeResult(entry, SyncEntryStatus.Rejected, "The gateway has no access to this alert.");
        }

        AlertChangeOutcome outcome;
        if (kind == AlertChangeKind.Evidence)
        {
            var (evidence, loadError) = await LoadReadingsAsync(entry.DeviceId ?? alert.DeviceId, entry.ReadingIds, cancellationToken);
            if (loadError is not null)
            {
                return ChangeResult(entry, SyncEntryStatus.Rejected, loadError);
            }

            outcome = alert.AddEvidence(entry.OperationId, evidence, command.ReceivedAt);
        }
        else
        {
            var (eventTime, timeError) = RequireEventTime(entry.OccurredAt, command.ReceivedAt);
            if (timeError is not null)
            {
                return ChangeResult(entry, SyncEntryStatus.Rejected, timeError);
            }

            if (kind == AlertChangeKind.Cleared)
            {
                outcome = alert.Clear(entry.OperationId, eventTime, command.ReceivedAt);
            }
            else
            {
                var (acknowledgement, rejection) = await AcknowledgeFromEdgeAsync(alert, entry, eventTime, command.ReceivedAt, cancellationToken);
                if (rejection is not null)
                {
                    return ChangeResult(entry, SyncEntryStatus.Rejected, rejection);
                }

                outcome = acknowledgement;
            }
        }

        if (outcome == AlertChangeOutcome.Applied)
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            return ChangeResult(entry, SyncEntryStatus.Accepted, null, alert.Id);
        }

        return outcome == AlertChangeOutcome.AlreadyApplied
            ? ChangeResult(entry, SyncEntryStatus.Duplicate, "The change was already applied.", alert.Id)
            : ChangeResult(entry, SyncEntryStatus.Conflict, "The alert was changed by a different operation; the current attention was kept.", alert.Id);
    }

    private async Task<(AlertChangeOutcome Outcome, string? Rejection)> AcknowledgeFromEdgeAsync(
        MonitoringAlert alert,
        EdgeAlertChangeEntry entry,
        DateTimeOffset acknowledgedAt,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        if (entry.AuthorId is not { } authorId)
        {
            return (AlertChangeOutcome.Conflict, "Edge attention requires its author.");
        }

        if (entry.ExpectedVersion is not { } expectedVersion)
        {
            return (AlertChangeOutcome.Conflict, "Edge attention requires the expected alert version.");
        }

        // A retried operation keeps its original result even if the author's access changed afterwards.
        if (!alert.HasApplied(entry.OperationId)
            && !await accessControl.CanAccessRanchAsync(authorId, alert.RanchId, MonitoringPermission.AttendAlerts, cancellationToken))
        {
            return (AlertChangeOutcome.Conflict, "The account is not authorized for this ranch operation.");
        }

        var outcome = alert.Acknowledge(entry.OperationId, authorId, entry.Finding ?? string.Empty, entry.Action,
            acknowledgedAt, expectedVersion, receivedAt);
        return (outcome, null);
    }

    private async Task<(IReadOnlyList<TelemetryReading> Readings, string? Error)> LoadReadingsAsync(
        Guid deviceId,
        IReadOnlyList<Guid> readingIds,
        CancellationToken cancellationToken)
    {
        if (readingIds.Count == 0)
        {
            return ([], "At least one supporting reading is required.");
        }

        if (readingIds.Distinct().Count() != readingIds.Count)
        {
            return ([], "Supporting readings cannot be repeated.");
        }

        var found = (await readings.FindByDeviceAndReadingIdsAsync(deviceId, readingIds, cancellationToken)).ToDictionary(r => r.ReadingId);
        var ordered = new List<TelemetryReading>(readingIds.Count);
        foreach (var readingId in readingIds)
        {
            if (!found.TryGetValue(readingId, out var reading))
            {
                return ([], $"Supporting reading {readingId} has not been synchronized.");
            }

            ordered.Add(reading);
        }

        return (ordered, null);
    }

    private (DateTimeOffset EventTime, string? Error) RequireEventTime(DateTimeOffset? occurredAt, DateTimeOffset receivedAt)
    {
        if (occurredAt is not { } eventTime)
        {
            return (default, "The event time is required.");
        }

        var instant = MonitoringTime.Normalize(eventTime);
        return instant > receivedAt + options.Value.ClockSkewTolerance
            ? (default, "The event time is ahead of the server clock beyond the configured tolerance.")
            : (instant, null);
    }
}
