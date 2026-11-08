using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     An unknown alert type, severity or change kind becomes null, so that only its entry is rejected.
/// </summary>
public static class SynchronizeEdgeBatchCommandFromResourceAssembler
{
    public static SynchronizeEdgeBatchCommand ToCommandFromResource(EdgeBatchResource resource) =>
        new(
            (resource.Readings ?? []).Select(ToReadingEntry).ToList(),
            (resource.Alerts ?? []).Select(ToAlertEntry).ToList(),
            (resource.AlertChanges ?? []).Select(ToAlertChangeEntry).ToList());

    private static EdgeReadingEntry ToReadingEntry(EdgeReadingResource reading) =>
        new(
            reading.ReadingId,
            reading.DeviceId,
            reading.RecordedAt,
            ToCapturedMetric(reading.BodyTemperature),
            ToCapturedMetric(reading.ActivityIndex),
            ToCapturedMetric(reading.RuminationMinutes),
            ToCapturedMetric(reading.BatteryLevel),
            reading.Position is null
                ? null
                : new CapturedPosition(reading.Position.Latitude, reading.Position.Longitude, reading.Position.AccuracyMeters, reading.Position.InvalidReason));

    private static CapturedMetric? ToCapturedMetric(CapturedMetricResource? metric) =>
        metric is null ? null : new CapturedMetric(metric.Value, metric.InvalidReason);

    private static EdgeAlertEntry ToAlertEntry(EdgeAlertResource alert) =>
        new(
            new MonitoringAlertId(alert.AlertId),
            alert.DeviceId,
            MonitoringEnumParser.Parse<AlertType>(alert.Type),
            MonitoringEnumParser.Parse<AlertSeverity>(alert.Severity),
            alert.ReadingIds ?? [],
            alert.ConfigurationRevision,
            alert.ThresholdMinValue,
            alert.ThresholdMaxValue,
            alert.GeofenceId is { } geofenceId ? new GeofenceId(geofenceId) : (GeofenceId?)null,
            alert.DistanceToBoundaryMeters);

    private static EdgeAlertChangeEntry ToAlertChangeEntry(EdgeAlertChangeResource change) =>
        new(
            change.OperationId,
            new MonitoringAlertId(change.AlertId),
            MonitoringEnumParser.Parse<AlertChangeKind>(change.Kind),
            change.DeviceId,
            change.ReadingIds ?? [],
            change.OccurredAt,
            change.AuthorId,
            change.Finding,
            change.Action,
            change.ExpectedVersion);
}
