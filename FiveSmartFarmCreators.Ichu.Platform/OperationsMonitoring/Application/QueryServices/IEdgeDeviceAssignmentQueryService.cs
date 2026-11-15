using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;

public interface IEdgeDeviceAssignmentQueryService
{
    /// <summary>
    ///     Publishes the collar assignments of the gateway's ranch that serve an active animal, when the requested gateway is the verified one.
    /// </summary>
    Task<Result<EdgeDeviceAssignmentSnapshot>> Handle(GetEdgeDeviceAssignmentsQuery query, CancellationToken cancellationToken);
}
