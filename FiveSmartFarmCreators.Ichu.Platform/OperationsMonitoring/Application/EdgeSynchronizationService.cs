using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>
/// Validates and persists the readings, alerts and alert changes synchronized by an Edge Gateway (TS-06, TS-07).
/// Every entry is evaluated and saved independently so that one rejected entry never blocks the others.
/// </summary>
public sealed class EdgeSynchronizationService(
    MonitoringAuthorization authorization,
    SyncEntryProcessor entryProcessor,
    IDeviceAttributionLookup deviceAttributions,
    ICattleLookup cattleLookup,
    ITelemetryReadingRepository readings,
    IMonitoringAlertRepository alerts,
    IGeofenceRepository geofences,
    IMonitoringRuleSetRepository ruleSets,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock)
{
    public async Task<IReadOnlyList<SyncEntryResult>> SynchronizeAsync(SynchronizeEdgeBatchCommand command, CancellationToken cancellationToken)
    {
        var gateway = await authorization.RequireGatewayAsync(cancellationToken);
        var entryCount = command.Readings.Count + command.Alerts.Count + command.AlertChanges.Count;
        var maximum = options.Value.MaximumBatchEntries;
        if (entryCount == 0 || entryCount > maximum)
        {
            throw MonitoringException.InvalidInput($"A batch must contain between 1 and {maximum} entries.");
        }

        var receivedAt = clock.GetUtcNow();
        var results = new List<SyncEntryResult>(entryCount);

        // Readings are processed first so that alerts in the same batch can reference them as evidence.
        foreach (var entry in command.Readings)
        {
            results.Add(await entryProcessor.ProcessAsync(SyncEntryKind.Reading, entry.ReadingId,
                () => SynchronizeReadingAsync(gateway, entry, receivedAt, cancellationToken)));
        }

        foreach (var entry in command.Alerts)
        {
            results.Add(await entryProcessor.ProcessAsync(SyncEntryKind.Alert, entry.AlertId,
                () => SynchronizeAlertAsync(gateway, entry, receivedAt, cancellationToken)));
        }

        foreach (var entry in command.AlertChanges)
        {
            results.Add(await entryProcessor.ProcessAsync(SyncEntryKind.AlertChange, entry.OperationId,
                () => SynchronizeAlertChangeAsync(gateway, entry, receivedAt, cancellationToken)));
        }

        return results;
    }

    private async Task<SyncEntryResult> SynchronizeReadingAsync(
        EdgeGatewayPrincipal gateway,
        EdgeReadingEntry entry,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var recordedAt = entry.RecordedAt?.ToUniversalTime()
            ?? throw MonitoringException.InvalidInput("The capture time is required.");
        EnsureNotAhead(recordedAt, receivedAt, "capture time");

        var existing = await readings.FindAsync(entry.DeviceId, entry.ReadingId, cancellationToken);
        if (existing is not null)
        {
            var replay = CreateReading(entry, recordedAt, existing.GatewayId, existing.RanchId, existing.CattleId, existing.ReceivedAt);
            return existing.HasSameCaptureAs(replay)
                ? new SyncEntryResult(SyncEntryKind.Reading, entry.ReadingId, SyncEntryStatus.Duplicate, "The reading was already synchronized.")
                : new SyncEntryResult(SyncEntryKind.Reading, entry.ReadingId, SyncEntryStatus.Conflict, "A different reading already uses this identifier.");
        }

        var attribution = await deviceAttributions.FindAsync(entry.DeviceId, recordedAt, cancellationToken)
            ?? throw MonitoringException.NotFound("The device is not registered.");
        if (attribution.RanchId != gateway.RanchId)
        {
            throw MonitoringException.Forbidden("The gateway has no access to the ranch of this device.");
        }

        if (attribution.CattleId is { } cattleId)
        {
            var cattle = await cattleLookup.FindAsync(cattleId, cancellationToken);
            if (cattle is null || cattle.RanchId != gateway.RanchId || !cattle.WasActiveAt(recordedAt))
            {
                throw MonitoringException.InvalidInput("The assigned animal was not active in the herd at the capture time.");
            }
        }

        readings.Add(CreateReading(entry, recordedAt, gateway.GatewayId, gateway.RanchId, attribution.CattleId, receivedAt));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var note = attribution.CattleId is null
            ? "No device assignment was effective at the capture time; the reading is kept as device data and excluded from herd indicators."
            : null;
        return new SyncEntryResult(SyncEntryKind.Reading, entry.ReadingId, SyncEntryStatus.Accepted, note);
    }

    private async Task<SyncEntryResult> SynchronizeAlertAsync(
        EdgeGatewayPrincipal gateway,
        EdgeAlertEntry entry,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var type = MonitoringValueParser.ParseEnum<AlertType>(entry.Type, "alert type");
        var severity = MonitoringValueParser.ParseEnum<AlertSeverity>(entry.Severity, "alert severity");
        var supporting = await LoadReadingsAsync(entry.DeviceId, entry.ReadingIds, cancellationToken);
        if (supporting[0].RanchId != gateway.RanchId)
        {
            throw MonitoringException.Forbidden("The gateway has no access to the ranch of this alert.");
        }

        // An alert that previously joined an open alert is recorded there as evidence under its own identifier.
        if (await alerts.FindByOperationAsync(entry.AlertId, cancellationToken) is { } absorbing)
        {
            var replay = absorbing.AddEvidence(entry.AlertId, supporting, receivedAt);
            return replay == AlertChangeOutcome.AlreadyApplied
                ? new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Duplicate,
                    "The alert already joined an open alert of the same condition.", absorbing.Id)
                : new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Conflict,
                    "A different alert already uses this identifier.", absorbing.Id);
        }

        var candidate = await CreateAlertAsync(gateway, entry, type, severity, supporting, receivedAt, cancellationToken);
        var existing = await alerts.FindAsync(entry.AlertId, cancellationToken);
        if (existing is not null)
        {
            return existing.HasSameOriginAs(candidate)
                ? new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Duplicate, "The alert was already synchronized.")
                : new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Conflict, "A different alert already uses this identifier.");
        }

        var cattle = await cattleLookup.FindAsync(candidate.CattleId, cancellationToken);
        if (cattle is null || cattle.RanchId != gateway.RanchId || !cattle.WasActiveAt(candidate.RaisedAt))
        {
            throw MonitoringException.InvalidInput("An animal that is not active in the herd does not generate alerts.");
        }

        var pending = await alerts.FindPendingAsync(candidate.CattleId, type, cancellationToken);
        var open = pending.FirstOrDefault(a => a.IsOpenDuplicateCandidate(type, candidate.CattleId, candidate.RaisedAt));
        if (open is not null)
        {
            open.AddEvidence(entry.AlertId, supporting, receivedAt);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Duplicate,
                "The alert joined the open alert of the same condition.", open.Id);
        }

        alerts.Add(candidate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new SyncEntryResult(SyncEntryKind.Alert, entry.AlertId, SyncEntryStatus.Accepted, null);
    }

    private async Task<MonitoringAlert> CreateAlertAsync(
        EdgeGatewayPrincipal gateway,
        EdgeAlertEntry entry,
        AlertType type,
        AlertSeverity severity,
        IReadOnlyList<TelemetryReading> supporting,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var ruleSet = await ruleSets.FindAsync(gateway.RanchId, cancellationToken);
        if (ruleSet is null || entry.ConfigurationRevision > ruleSet.Revision)
        {
            throw MonitoringException.InvalidInput("The alert references a configuration revision that was never published.");
        }

        if (type == AlertType.Geofence)
        {
            var geofenceId = entry.GeofenceId ?? throw MonitoringException.InvalidInput("A geofence alert requires its geofence.");
            var geofence = await geofences.FindAsync(geofenceId, cancellationToken);
            if (geofence is null || geofence.RanchId != gateway.RanchId)
            {
                throw MonitoringException.NotFound("The geofence does not exist for this ranch.");
            }

            var distance = entry.DistanceToBoundaryMeters
                ?? throw MonitoringException.InvalidInput("A geofence alert requires the distance to the boundary.");
            return MonitoringAlert.RaiseForGeofence(entry.AlertId, severity, geofenceId, distance,
                entry.ConfigurationRevision, gateway.GatewayId, supporting, receivedAt);
        }

        if (entry.ThresholdMinValue is not { } min || entry.ThresholdMaxValue is not { } max)
        {
            throw MonitoringException.InvalidInput("A threshold alert requires the threshold used to evaluate it.");
        }

        return MonitoringAlert.RaiseForThreshold(entry.AlertId, type, severity, new ThresholdRange(min, max),
            entry.ConfigurationRevision, gateway.GatewayId, supporting, receivedAt);
    }

    private async Task<SyncEntryResult> SynchronizeAlertChangeAsync(
        EdgeGatewayPrincipal gateway,
        EdgeAlertChangeEntry entry,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var kind = MonitoringValueParser.ParseEnum<AlertChangeKind>(entry.Kind, "change kind");
        var alert = await alerts.FindAsync(entry.AlertId, cancellationToken)
            ?? throw MonitoringException.NotFound("The alert has not been synchronized.");
        if (alert.RanchId != gateway.RanchId)
        {
            throw MonitoringException.Forbidden("The gateway has no access to this alert.");
        }

        AlertChangeOutcome outcome;
        if (kind == AlertChangeKind.Evidence)
        {
            var evidence = await LoadReadingsAsync(entry.DeviceId ?? alert.DeviceId, entry.ReadingIds, cancellationToken);
            outcome = alert.AddEvidence(entry.OperationId, evidence, receivedAt);
        }
        else if (kind == AlertChangeKind.Cleared)
        {
            outcome = alert.Clear(entry.OperationId, RequireEventTime(entry.OccurredAt, receivedAt), receivedAt);
        }
        else
        {
            outcome = await AcknowledgeFromEdgeAsync(alert, entry, receivedAt, cancellationToken);
        }

        if (outcome == AlertChangeOutcome.Applied)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new SyncEntryResult(SyncEntryKind.AlertChange, entry.OperationId, SyncEntryStatus.Accepted, null, alert.Id);
        }

        return outcome == AlertChangeOutcome.AlreadyApplied
            ? new SyncEntryResult(SyncEntryKind.AlertChange, entry.OperationId, SyncEntryStatus.Duplicate, "The change was already applied.", alert.Id)
            : new SyncEntryResult(SyncEntryKind.AlertChange, entry.OperationId, SyncEntryStatus.Conflict,
                "The alert was changed by a different operation; the current attention was kept.", alert.Id);
    }

    private async Task<AlertChangeOutcome> AcknowledgeFromEdgeAsync(
        MonitoringAlert alert,
        EdgeAlertChangeEntry entry,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var authorId = entry.AuthorId ?? throw MonitoringException.InvalidInput("Edge attention requires its author.");
        var expectedVersion = entry.ExpectedVersion
            ?? throw MonitoringException.InvalidInput("Edge attention requires the expected alert version.");
        var acknowledgedAt = RequireEventTime(entry.OccurredAt, receivedAt);

        // A retried operation keeps its original result even if the author's access changed afterwards.
        if (!alert.HasApplied(entry.OperationId))
        {
            await authorization.EnsureAsync(authorId, alert.RanchId, MonitoringPermission.AttendAlerts, cancellationToken);
        }

        return alert.Acknowledge(entry.OperationId, authorId, entry.Finding ?? string.Empty, entry.Action,
            acknowledgedAt, expectedVersion, receivedAt);
    }

    private async Task<IReadOnlyList<TelemetryReading>> LoadReadingsAsync(
        Guid deviceId,
        IReadOnlyList<Guid> readingIds,
        CancellationToken cancellationToken)
    {
        if (readingIds.Count == 0)
        {
            throw MonitoringException.InvalidInput("At least one supporting reading is required.");
        }

        if (readingIds.Distinct().Count() != readingIds.Count)
        {
            throw MonitoringException.InvalidInput("Supporting readings cannot be repeated.");
        }

        var found = (await readings.FindManyAsync(deviceId, readingIds, cancellationToken)).ToDictionary(r => r.ReadingId);
        var ordered = new List<TelemetryReading>(readingIds.Count);
        foreach (var readingId in readingIds)
        {
            if (!found.TryGetValue(readingId, out var reading))
            {
                throw MonitoringException.NotFound($"Supporting reading {readingId} has not been synchronized.");
            }

            ordered.Add(reading);
        }

        return ordered;
    }

    private DateTimeOffset RequireEventTime(DateTimeOffset? occurredAt, DateTimeOffset receivedAt)
    {
        var instant = occurredAt?.ToUniversalTime() ?? throw MonitoringException.InvalidInput("The event time is required.");
        EnsureNotAhead(instant, receivedAt, "event time");
        return instant;
    }

    private void EnsureNotAhead(DateTimeOffset instant, DateTimeOffset receivedAt, string field)
    {
        if (instant > receivedAt + options.Value.ClockSkewTolerance)
        {
            throw MonitoringException.InvalidInput($"The {field} is ahead of the server clock beyond the configured tolerance.");
        }
    }

    private static TelemetryReading CreateReading(
        EdgeReadingEntry entry,
        DateTimeOffset recordedAt,
        Guid gatewayId,
        Guid ranchId,
        Guid? cattleId,
        DateTimeOffset receivedAt) =>
        new(
            entry.ReadingId,
            entry.DeviceId,
            recordedAt,
            ToMeasurement(entry.BodyTemperature),
            ToMeasurement(entry.ActivityIndex),
            ToMeasurement(entry.RuminationMinutes),
            ToMeasurement(entry.BatteryPercent),
            entry.Position is { } position
                ? PositionMeasurement.From(position.Latitude, position.Longitude, position.AccuracyMeters, position.InvalidReason)
                : PositionMeasurement.Absent(),
            gatewayId,
            ranchId,
            cattleId,
            receivedAt);

    private static Measurement ToMeasurement(CapturedMetric? metric) =>
        metric is null ? Measurement.Absent() : Measurement.From(metric.Value, metric.InvalidReason);
}
