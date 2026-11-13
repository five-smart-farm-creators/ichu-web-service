using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class EdgeDeviceAssignmentResourceFromEntityAssembler
{
    public static EdgeDeviceAssignmentResource ToResourceFromEntity(EdgeDeviceAssignment assignment) =>
        new(assignment.DeviceId, assignment.CattleId, assignment.LotId, assignment.LifeStage);
}
