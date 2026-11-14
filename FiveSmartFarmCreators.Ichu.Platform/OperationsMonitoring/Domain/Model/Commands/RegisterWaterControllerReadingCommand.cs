using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record RegisterWaterControllerReadingCommand(Guid GatewayId, Guid RanchId, Guid ControllerId,
    Guid WaterTroughId, Guid ReadingId, decimal Temperature, WaterTemperatureUnit TemperatureUnit, bool IsValid,
    WaterHeaterState HeaterState, DateTimeOffset CapturedAt, DateTimeOffset ReceivedAt);
