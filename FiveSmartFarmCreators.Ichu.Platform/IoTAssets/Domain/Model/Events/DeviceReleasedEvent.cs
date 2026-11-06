using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Events;

public sealed record DeviceReleasedEvent(Guid EventId, DateTimeOffset OccurredAt, Guid RanchId, Guid DeviceId, Guid AssignmentId,
    Guid TargetId, AssignmentTargetType TargetType, string Reason) : IEvent;
