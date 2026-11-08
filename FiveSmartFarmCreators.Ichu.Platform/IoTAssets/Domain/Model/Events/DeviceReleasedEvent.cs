using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Events;

/// <summary>
///     Raised when a device stops serving its target, with the reason that closed the assignment.
/// </summary>
public sealed record DeviceReleasedEvent(
    Guid RanchId,
    DeviceId DeviceId,
    DeviceAssignmentId AssignmentId,
    Guid TargetId,
    AssignmentTargetType TargetType,
    string Reason) : IEvent;
