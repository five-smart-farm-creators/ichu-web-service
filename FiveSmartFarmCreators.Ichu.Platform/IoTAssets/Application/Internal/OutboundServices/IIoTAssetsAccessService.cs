using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>Checks trusted caller identity and current permissions before accessing ranch data.</summary>
public interface IIoTAssetsAccessService
{
    Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);
    Task<Result> EnsureCommunicationAccessAsync(Guid ranchId, Guid deviceId, CancellationToken cancellationToken);
    Task<Result> EnsureCattleLifecycleAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
