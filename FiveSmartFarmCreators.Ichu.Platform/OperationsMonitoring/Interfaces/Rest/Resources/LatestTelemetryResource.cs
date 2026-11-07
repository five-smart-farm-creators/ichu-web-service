namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the latest valid value of each indicator of an animal and its last known position.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
/// <param name="EvaluatedAt">The instant used to evaluate the age of the readings.</param>
/// <param name="LastReadingRecordedAt">The capture time of the most recent reading of any kind.</param>
/// <param name="Origin">The component that relayed the readings.</param>
/// <param name="BodyTemperature">The latest valid body temperature.</param>
/// <param name="ActivityIndex">The latest valid activity index.</param>
/// <param name="RuminationMinutes">The latest valid rumination minutes.</param>
/// <param name="LastKnownPosition">The last known position.</param>
public record LatestTelemetryResource(
    Guid AnimalId,
    DateTimeOffset EvaluatedAt,
    DateTimeOffset? LastReadingRecordedAt,
    string Origin,
    LatestIndicatorResource BodyTemperature,
    LatestIndicatorResource ActivityIndex,
    LatestIndicatorResource RuminationMinutes,
    LastKnownPositionResource LastKnownPosition);
