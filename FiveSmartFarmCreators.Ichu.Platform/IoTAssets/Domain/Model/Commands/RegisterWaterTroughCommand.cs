namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record RegisterWaterTroughCommand(Guid RanchId, string Name, Guid? LotId);
