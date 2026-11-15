namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Anti-corruption port towards IoT Assets and Cattle Information.
/// </summary>
public interface IEdgeDeviceAssignmentLookup
{
    /// <summary>
    ///     Finds the collars of the ranch whose animal is active, since an animal that has left the herd is no longer monitored.
    /// </summary>
    Task<IReadOnlyList<EdgeDeviceAssignment>> FindAsync(Guid ranchId, CancellationToken cancellationToken);
}
