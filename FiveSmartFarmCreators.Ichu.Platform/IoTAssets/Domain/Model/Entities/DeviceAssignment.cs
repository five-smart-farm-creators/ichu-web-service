using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;

/// <summary>
///     One period in which a device served a target. Ended periods stay in the history with the reason of their release.
/// </summary>
public class DeviceAssignment
{
    // Entity Framework Core materializes assignments without running the constructor.
    private DeviceAssignment()
    {
    }

    internal DeviceAssignment(DeviceId deviceId, Guid ranchId, Guid targetId, AssignmentTargetType targetType,
        DateTimeOffset startedAt)
    {
        Id = new DeviceAssignmentId(Guid.NewGuid());
        DeviceId = deviceId;
        RanchId = ranchId;
        TargetId = targetId;
        TargetType = targetType;
        StartedAt = startedAt.ToUniversalTime();
    }

    public DeviceAssignmentId Id { get; private set; }

    public DeviceId DeviceId { get; private set; }

    public Guid RanchId { get; private set; }

    public Guid TargetId { get; private set; }

    public AssignmentTargetType TargetType { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public string? ReleaseReason { get; private set; }

    public bool IsOpen() => EndedAt is null;

    /// <summary>
    ///     Checks the interval that the start includes and the end excludes, so that a release and a new assignment at the same instant do not overlap.
    /// </summary>
    public bool CoversInstant(DateTimeOffset instant) =>
        instant >= StartedAt && (EndedAt is null || instant < EndedAt);

    internal Result Close(string reason, DateTimeOffset endedAt)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 120 || endedAt == default)
        {
            return Result.Failure(IoTAssetsErrors.InvalidInput);
        }

        if (!IsOpen() || endedAt < StartedAt)
        {
            return Result.Failure(IoTAssetsErrors.InvalidState);
        }

        EndedAt = endedAt.ToUniversalTime();
        ReleaseReason = reason.Trim();
        return Result.Success();
    }
}
