using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>
/// The most recent valid value of each indicator, each from its own reading. A missing reading means that no
/// valid measurement exists; staleness is evaluated against <see cref="EvaluatedAt"/>.
/// </summary>
public sealed record LatestTelemetry(
    Guid AnimalId,
    DateTimeOffset EvaluatedAt,
    TelemetryReading? LastReading,
    TelemetryReading? BodyTemperature,
    TelemetryReading? ActivityIndex,
    TelemetryReading? RuminationMinutes,
    TelemetryReading? LastKnownPosition);
