using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Events;

/// <summary>
///     Raised when a device starts serving a cattle or a water trough target.
/// </summary>
public sealed record DeviceAssignedEvent(
    Guid RanchId,
    DeviceId DeviceId,
    DeviceAssignmentId AssignmentId,
    Guid TargetId,
    AssignmentTargetType TargetType) : IEvent;
