using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record LatestTelemetryResource(
    Guid AnimalId,
    DateTimeOffset EvaluatedAt,
    DateTimeOffset? LastReadingRecordedAt,
    string Origin,
    LatestIndicatorResource BodyTemperature,
    LatestIndicatorResource ActivityIndex,
    LatestIndicatorResource RuminationMinutes,
    LastKnownPositionResource LastKnownPosition)
{
    public static LatestTelemetryResource From(LatestTelemetry latest)
    {
        var now = latest.EvaluatedAt;
        return new LatestTelemetryResource(
            latest.AnimalId,
            now,
            latest.LastReading?.RecordedAt,
            MonitoringUnits.EdgeGatewayOrigin,
            LatestIndicatorResource.From(latest.BodyTemperature, r => r.BodyTemperature, MonitoringUnits.Celsius, now),
            LatestIndicatorResource.From(latest.ActivityIndex, r => r.ActivityIndex, MonitoringUnits.Index, now),
            LatestIndicatorResource.From(latest.RuminationMinutes, r => r.RuminationMinutes, MonitoringUnits.Minutes, now),
            LastKnownPositionResource.From(latest.LastKnownPosition, now));
    }
}
