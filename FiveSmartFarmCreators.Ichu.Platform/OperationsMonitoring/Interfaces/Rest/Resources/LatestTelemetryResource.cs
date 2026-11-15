namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record LatestTelemetryResource(
    Guid CattleId,
    DateTimeOffset EvaluatedAt,
    DateTimeOffset? LastReadingRecordedAt,
    string Origin,
    LatestIndicatorResource BodyTemperature,
    LatestIndicatorResource ActivityIndex,
    LatestIndicatorResource HeartRate,
    LatestIndicatorResource MethaneIndex,
    LatestIndicatorResource RuminationMinutes,
    LastKnownPositionResource LastKnownPosition);
