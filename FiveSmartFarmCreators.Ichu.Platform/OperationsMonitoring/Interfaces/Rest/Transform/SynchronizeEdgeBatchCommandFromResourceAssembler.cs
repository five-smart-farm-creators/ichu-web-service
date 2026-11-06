using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the edge batch synchronization command from its REST resource.
/// </summary>
public static class SynchronizeEdgeBatchCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms an <see cref="EdgeBatchResource"/> into a <see cref="SynchronizeEdgeBatchCommand"/>. An unknown
    ///     alert type, severity or change kind becomes <see langword="null"/>, so that only its entry is rejected.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the batch.</param>
    /// <returns>The command with the readings, alerts and alert changes.</returns>
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
            alert.AlertId,
            alert.DeviceId,
            Enum.GetNames<AlertType>().FirstOrDefault(n => n.Equals(alert.Type?.Trim(), StringComparison.OrdinalIgnoreCase)) is { } type
                ? Enum.Parse<AlertType>(type)
                : null,
            Enum.GetNames<AlertSeverity>().FirstOrDefault(n => n.Equals(alert.Severity?.Trim(), StringComparison.OrdinalIgnoreCase)) is { } severity
                ? Enum.Parse<AlertSeverity>(severity)
                : null,
            alert.ReadingIds ?? [],
            alert.ConfigurationRevision,
            alert.ThresholdMinValue,
            alert.ThresholdMaxValue,
            alert.GeofenceId,
            alert.DistanceToBoundaryMeters);

    private static EdgeAlertChangeEntry ToAlertChangeEntry(EdgeAlertChangeResource change) =>
        new(
            change.OperationId,
            change.AlertId,
            Enum.GetNames<AlertChangeKind>().FirstOrDefault(n => n.Equals(change.Kind?.Trim(), StringComparison.OrdinalIgnoreCase)) is { } kind
                ? Enum.Parse<AlertChangeKind>(kind)
                : null,
            change.DeviceId,
            change.ReadingIds ?? [],
            change.OccurredAt,
            change.AuthorId,
            change.Finding,
            change.Action,
            change.ExpectedVersion);
}
