using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="LatestTelemetry"/> value objects.
/// </summary>
public static class LatestTelemetryResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="LatestTelemetry"/> into a <see cref="LatestTelemetryResource"/>. An indicator
    ///     without a reading is reported as unavailable, never as zero.
    /// </summary>
    /// <param name="latest">The latest valid indicators of an animal.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static LatestTelemetryResource ToResourceFromEntity(LatestTelemetry latest)
    {
        var now = latest.EvaluatedAt;
        return new LatestTelemetryResource(
            latest.AnimalId,
            now,
            latest.LastReading?.RecordedAt,
            MonitoringUnits.EdgeGatewayOrigin,
            ToIndicator(latest.BodyTemperature, r => r.BodyTemperature, MonitoringUnits.Celsius, now),
            ToIndicator(latest.ActivityIndex, r => r.ActivityIndex, MonitoringUnits.Index, now),
            ToIndicator(latest.RuminationMinutes, r => r.RuminationMinutes, MonitoringUnits.Minutes, now),
            ToLastKnownPosition(latest.LastKnownPosition, now));
    }

    private static LatestIndicatorResource ToIndicator(TelemetryReading? reading, Func<TelemetryReading, Measurement> select, string unit, DateTimeOffset now) =>
        reading is null
            ? new LatestIndicatorResource("Unavailable", null, unit, null, null)
            : new LatestIndicatorResource("Available", select(reading).Value, unit, reading.RecordedAt, reading.IsStale(now));

    private static LastKnownPositionResource ToLastKnownPosition(TelemetryReading? reading, DateTimeOffset now) =>
        reading is null
            ? new LastKnownPositionResource("Unavailable", null, null, null, null, null)
            : new LastKnownPositionResource("Available", reading.Position.Latitude, reading.Position.Longitude,
                reading.Position.AccuracyMeters, reading.RecordedAt, reading.IsStale(now));
}
