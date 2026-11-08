using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record ConfigureWaterTroughTemperatureCommand(
    Guid RanchId,
    WaterTroughId WaterTroughId,
    decimal MinimumCelsius,
    decimal MaximumCelsius,
    Guid ExpectedVersion);
