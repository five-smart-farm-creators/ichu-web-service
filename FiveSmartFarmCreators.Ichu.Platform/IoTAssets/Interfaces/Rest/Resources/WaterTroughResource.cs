namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record WaterTroughResource(Guid Id, Guid RanchId, string Name, Guid? LotId, TemperatureRangeResource? TemperatureRange, Guid Version);
