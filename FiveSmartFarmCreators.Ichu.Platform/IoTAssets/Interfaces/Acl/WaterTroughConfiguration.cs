namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

public sealed record WaterTroughConfiguration(Guid Id, Guid RanchId, string Name, Guid? LotId,
    decimal? MinimumCelsius, decimal? MaximumCelsius, Guid Version);
