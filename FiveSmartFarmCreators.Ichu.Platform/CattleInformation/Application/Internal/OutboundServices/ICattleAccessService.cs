using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
///     Checks that the current caller may read or manage a ranch's animals before any data is loaded.
/// </summary>
public interface ICattleAccessService
{
    Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
