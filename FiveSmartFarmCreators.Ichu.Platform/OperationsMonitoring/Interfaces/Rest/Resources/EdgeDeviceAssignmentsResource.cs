namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record EdgeDeviceAssignmentsResource(Guid RanchId, IReadOnlyList<EdgeDeviceAssignmentResource> Assignments);
