using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

public enum WaterControllerStatus
{
    NoController,
    NoReadings,
    Communicating,
    NoCommunication
}

public enum WaterMeasurementStatus
{
    Unavailable,
    Invalid,
    Valid
}

public enum WaterTemperatureRangeStatus
{
    NotConfigured,
    Unavailable,
    BelowRange,
    WithinRange,
    AboveRange
}

public sealed record WaterTroughStatus(
    Guid WaterTroughId,
    string Name,
    Guid? LotId,
    decimal? MinimumCelsius,
    decimal? MaximumCelsius,
    WaterControllerStatus ControllerStatus,
    Guid? ControllerId,
    DateTimeOffset? ControllerAssignedAt,
    DateTimeOffset? LastCommunicationAt,
    WaterMeasurementStatus MeasurementStatus,
    WaterTemperatureRangeStatus RangeStatus,
    WaterTroughReading? LatestReading,
    DateTimeOffset EvaluatedAt);
