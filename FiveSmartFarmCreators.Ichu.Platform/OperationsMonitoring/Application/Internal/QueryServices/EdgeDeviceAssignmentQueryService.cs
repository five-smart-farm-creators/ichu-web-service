using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Publishes the collar assignments of a ranch to its Edge Gateways, under the same gateway checks as the monitoring rules.
/// </summary>
public class EdgeDeviceAssignmentQueryService(
    IEdgeGatewayAuthenticator gatewayAuthenticator,
    IEdgeDeviceAssignmentLookup assignmentLookup) : IEdgeDeviceAssignmentQueryService
{
    /// <inheritdoc />
    public async Task<Result<EdgeDeviceAssignmentSnapshot>> Handle(GetEdgeDeviceAssignmentsQuery query, CancellationToken cancellationToken)
    {
        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return Result<EdgeDeviceAssignmentSnapshot>.Failure(OperationsMonitoringErrors.GatewayUnauthenticated);
        }

        if (gateway.GatewayId != query.GatewayId)
        {
            return Result<EdgeDeviceAssignmentSnapshot>.Failure(OperationsMonitoringErrors.GatewayMismatch);
        }

        // Ordering by device keeps consecutive downloads comparable, whatever order the database returns.
        var assignments = await assignmentLookup.FindAsync(gateway.RanchId, cancellationToken);
        return Result<EdgeDeviceAssignmentSnapshot>.Success(
            new EdgeDeviceAssignmentSnapshot(gateway.RanchId, assignments.OrderBy(assignment => assignment.DeviceId).ToList()));
    }
}
