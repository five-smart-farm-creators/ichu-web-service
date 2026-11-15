using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record WaterTroughReadingResource(
    Guid Id,
    Guid ReadingId,
    Guid ControllerId,
    Guid WaterTroughId,
    Guid RanchId,
    Guid GatewayId,
    decimal Temperature,
    WaterTemperatureUnit TemperatureUnit,
    bool IsValid,
    WaterHeaterState HeaterState,
    DateTimeOffset CapturedAt,
    DateTimeOffset ReceivedAt,
    string Origin);
