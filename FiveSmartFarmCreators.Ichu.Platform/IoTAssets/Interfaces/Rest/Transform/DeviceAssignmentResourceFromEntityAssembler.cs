using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class DeviceAssignmentResourceFromEntityAssembler
{
    public static DeviceAssignmentResource ToResourceFromEntity(DeviceAssignment assignment) =>
        new(assignment.Id, assignment.DeviceId, assignment.RanchId, assignment.TargetId, assignment.TargetType.ToString(),
            assignment.StartedAt, assignment.EndedAt, assignment.ReleaseReason);
}
