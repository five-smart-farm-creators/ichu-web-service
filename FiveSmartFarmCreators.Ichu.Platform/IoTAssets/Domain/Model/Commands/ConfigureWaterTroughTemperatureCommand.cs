namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record ConfigureWaterTroughTemperatureCommand(Guid RanchId, Guid WaterTroughId, decimal MinimumCelsius, decimal MaximumCelsius, Guid ExpectedVersion);
