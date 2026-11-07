namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

public sealed record DeviceAssignmentSnapshot(Guid Id, Guid DeviceId, Guid RanchId, Guid TargetId,
    string TargetType, DateTimeOffset StartedAt, DateTimeOffset? EndedAt);
