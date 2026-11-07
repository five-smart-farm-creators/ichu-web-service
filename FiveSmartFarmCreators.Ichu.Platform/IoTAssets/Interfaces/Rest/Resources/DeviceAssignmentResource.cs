namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record DeviceAssignmentResource(Guid Id, Guid DeviceId, Guid RanchId, Guid TargetId, string TargetType,
    DateTimeOffset StartedAt, DateTimeOffset? EndedAt, string? ReleaseReason);
