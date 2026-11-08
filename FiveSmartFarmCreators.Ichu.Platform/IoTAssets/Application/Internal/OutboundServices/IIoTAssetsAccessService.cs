using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>
///     Checks the trusted caller before any ranch data is read or changed.
/// </summary>
public interface IIoTAssetsAccessService
{
    Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    ///     Checks a device report from an edge gateway, which has no user account, so the device must belong to the ranch.
    /// </summary>
    Task<Result> EnsureCommunicationAccessAsync(Guid ranchId, Guid deviceId, CancellationToken cancellationToken);
}
