namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

public sealed record DeviceAttributionSnapshot(Guid DeviceId, Guid RanchId, Guid? CattleId);
