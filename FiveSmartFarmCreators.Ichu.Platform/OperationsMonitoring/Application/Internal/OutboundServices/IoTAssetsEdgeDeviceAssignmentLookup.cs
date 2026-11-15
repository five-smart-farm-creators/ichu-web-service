using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Joins the collars that IoT Assets assigns to animals with the active animals of Cattle Information, so that the collars of exited animals are left out.
/// </summary>
public class IoTAssetsEdgeDeviceAssignmentLookup(
    IIoTAssetsContextFacade iotAssets,
    ICattleInformationContextFacade cattleInformation) : IEdgeDeviceAssignmentLookup
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<EdgeDeviceAssignment>> FindAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var assignments = await iotAssets.FindOpenCollarAssignmentsAsync(ranchId, cancellationToken);
        var placements = await cattleInformation.FindActivePlacementsAsync(ranchId, cancellationToken);
        return (from assignment in assignments
                join placement in placements on assignment.TargetId equals placement.CattleId
                select new EdgeDeviceAssignment(assignment.DeviceId, placement.CattleId, placement.LotId, placement.LifeStage))
            .ToList();
    }
}
