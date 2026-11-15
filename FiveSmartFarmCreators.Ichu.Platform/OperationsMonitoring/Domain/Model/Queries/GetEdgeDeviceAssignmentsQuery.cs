namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Asks for the collar assignments that an Edge Gateway applies to the readings of its ranch.
/// </summary>
public record GetEdgeDeviceAssignmentsQuery(Guid GatewayId);
