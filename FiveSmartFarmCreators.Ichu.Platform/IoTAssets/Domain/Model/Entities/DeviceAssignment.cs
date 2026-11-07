using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;

public class DeviceAssignment
{
    private DeviceAssignment()
    {
    }

    internal DeviceAssignment(Guid deviceId, Guid ranchId, Guid targetId, AssignmentTargetType targetType,
        DateTimeOffset startedAt)
    {
        if (deviceId == Guid.Empty || ranchId == Guid.Empty || targetId == Guid.Empty
            || !Enum.IsDefined(targetType) || startedAt == default)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        Id = Guid.NewGuid();
        DeviceId = deviceId;
        RanchId = ranchId;
        TargetId = targetId;
        TargetType = targetType;
        StartedAt = startedAt.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid DeviceId { get; private set; }
    public Guid RanchId { get; private set; }
    public Guid TargetId { get; private set; }
    public AssignmentTargetType TargetType { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? EndedAt { get; private set; }
    public string? ReleaseReason { get; private set; }

    public bool IsOpen() => EndedAt is null;

    public bool CoversInstant(DateTimeOffset instant) =>
        instant >= StartedAt && (EndedAt is null || instant < EndedAt);

    internal void Close(string reason, DateTimeOffset endedAt)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 120 || endedAt == default)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        if (!IsOpen() || endedAt < StartedAt)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidState);
        }

        EndedAt = endedAt.ToUniversalTime();
        ReleaseReason = reason.Trim();
    }
}
