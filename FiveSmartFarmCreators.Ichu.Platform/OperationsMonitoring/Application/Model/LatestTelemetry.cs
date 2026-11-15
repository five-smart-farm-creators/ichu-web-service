using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     The most recent valid value of each indicator, each from its own reading. A missing reading means that no valid measurement exists, and staleness is evaluated against the evaluation instant.
/// </summary>
public record LatestTelemetry(
    Guid AnimalId,
    DateTimeOffset EvaluatedAt,
    TelemetryReading? LastReading,
    TelemetryReading? BodyTemperature,
    TelemetryReading? ActivityIndex,
    TelemetryReading? HeartRate,
    TelemetryReading? MethaneIndex,
    TelemetryReading? RuminationMinutes,
    TelemetryReading? LastKnownPosition);
