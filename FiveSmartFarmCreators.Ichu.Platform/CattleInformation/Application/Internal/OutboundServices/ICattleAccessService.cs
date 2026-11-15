using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
///     Checks whether the current caller may read or manage a ranch's animals.
/// </summary>
public interface ICattleAccessService
{
    Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
