using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class EdgeDeviceAssignmentsResourceFromEntityAssembler
{
    public static EdgeDeviceAssignmentsResource ToResourceFromEntity(EdgeDeviceAssignmentSnapshot snapshot) =>
        new(
            snapshot.RanchId,
            snapshot.Assignments.Select(EdgeDeviceAssignmentResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
