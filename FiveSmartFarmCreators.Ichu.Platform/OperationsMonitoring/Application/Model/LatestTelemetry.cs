using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Read model of the most recent valid value of each indicator, each from its own reading. A missing
///     reading means that no valid measurement exists; staleness is evaluated against <paramref name="EvaluatedAt"/>.
/// </summary>
/// <param name="AnimalId">The animal the readings belong to.</param>
/// <param name="EvaluatedAt">The instant used to evaluate the age of the readings.</param>
/// <param name="LastReading">The most recent reading of any kind.</param>
/// <param name="BodyTemperature">The reading with the latest valid body temperature.</param>
/// <param name="ActivityIndex">The reading with the latest valid activity index.</param>
/// <param name="RuminationMinutes">The reading with the latest valid rumination minutes.</param>
/// <param name="LastKnownPosition">The reading with the latest valid position.</param>
public record LatestTelemetry(
    Guid AnimalId,
    DateTimeOffset EvaluatedAt,
    TelemetryReading? LastReading,
    TelemetryReading? BodyTemperature,
    TelemetryReading? ActivityIndex,
    TelemetryReading? RuminationMinutes,
    TelemetryReading? LastKnownPosition);
