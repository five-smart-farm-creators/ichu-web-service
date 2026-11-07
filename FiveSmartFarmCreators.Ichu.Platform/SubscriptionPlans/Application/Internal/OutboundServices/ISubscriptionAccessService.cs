using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;

/// <summary>
///     Checks that the current caller may act on a ranch. Each check returns the verified account of the caller.
/// </summary>
public interface ISubscriptionAccessService
{
    Task<Result<Guid>> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<Result<Guid>> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken);
}
