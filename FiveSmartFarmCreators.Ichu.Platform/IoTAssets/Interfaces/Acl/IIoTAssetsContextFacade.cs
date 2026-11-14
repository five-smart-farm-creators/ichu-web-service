using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

/// <summary>Exposes authorized device facts and integration operations without sharing IoT domain entities.</summary>
public interface IIoTAssetsContextFacade
{
    /// <summary>Returns the current collar assignment, or null when no collar is currently assigned.</summary>
    Task<Result<DeviceAssignmentSnapshot?>> GetCurrentCollarAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);

    /// <summary>Resolves the assignment at capture time using a start-inclusive, end-exclusive interval.</summary>
    Task<Result<DeviceAssignmentSnapshot?>> ResolveAssignmentAtAsync(Guid ranchId, Guid deviceId,
        DateTimeOffset instant, CancellationToken cancellationToken);

    /// <summary>Returns active assignments for authorized configuration consumers; it does not implement the Edge API.</summary>
    Task<Result<IReadOnlyList<DeviceAssignmentSnapshot>>> GetActiveAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Result<WaterTroughConfiguration>> GetWaterTroughConfigurationAsync(Guid ranchId, Guid waterTroughId,
        CancellationToken cancellationToken);

    /// <summary>Accepts validated communication facts from an authorized integration, retaining original Edge and backend receipt times.</summary>
    Task<Result> RegisterCommunicationAsync(Guid ranchId, Guid deviceId, DateTimeOffset communicatedAt,
        DateTimeOffset receivedAt, int? batteryPercentage, CancellationToken cancellationToken);

    /// <summary>Releases the inactive animal's current collar; a repeated notification returns zero without changing history.</summary>
    Task<Result<int>> ReleaseCollarForInactiveCattleAsync(Guid ranchId, Guid cattleId, DateTimeOffset deactivatedAt,
        CancellationToken cancellationToken);

    /// <summary>Resolves the edge gateway of a presented key, for trusted in-process callers that authorize the result themselves.</summary>
    /// <returns>The gateway, or null when the key is malformed, unknown, wrong or revoked.</returns>
    Task<EdgeGatewaySnapshot?> AuthenticateEdgeGatewayAsync(string presentedKey, CancellationToken cancellationToken);

    /// <summary>Finds the animal that a device served at an instant, for trusted in-process callers that authorize the result themselves.</summary>
    /// <returns>The device attribution, or null when the device does not exist.</returns>
    Task<DeviceAttributionSnapshot?> FindDeviceAttributionAsync(Guid deviceId, DateTimeOffset instant, CancellationToken cancellationToken);

    /// <summary>Resolves a water controller and its historical trough assignment for a trusted integration at capture time.</summary>
    Task<WaterControllerAssignmentSnapshot?> FindWaterControllerAssignmentAsync(Guid controllerId,
        DateTimeOffset instant, CancellationToken cancellationToken);

    /// <summary>Returns water trough facts and current controller assignments after enforcing IoT Assets read access for the ranch.</summary>
    Task<Result<IReadOnlyList<WaterTroughMonitoringSnapshot>>> GetWaterTroughMonitoringSnapshotsAsync(Guid ranchId,
        CancellationToken cancellationToken);

    /// <summary>Lists the open assignments of the ranch's collars to animals, for trusted in-process callers that authorize the result themselves.</summary>
    Task<IReadOnlyList<DeviceAssignmentSnapshot>> FindOpenCollarAssignmentsAsync(Guid ranchId, CancellationToken cancellationToken);
}
