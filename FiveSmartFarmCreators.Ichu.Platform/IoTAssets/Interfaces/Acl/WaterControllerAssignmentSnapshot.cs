namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

public sealed record WaterControllerAssignmentSnapshot(Guid ControllerId, Guid RanchId, bool IsWaterController,
    Guid? WaterTroughId, DateTimeOffset? AssignedAt);
