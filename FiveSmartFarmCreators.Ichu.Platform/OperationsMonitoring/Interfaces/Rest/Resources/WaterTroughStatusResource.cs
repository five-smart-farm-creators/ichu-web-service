using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record WaterTroughStatusResource(
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
    WaterTroughReadingResource? LatestReading,
    DateTimeOffset EvaluatedAt);
