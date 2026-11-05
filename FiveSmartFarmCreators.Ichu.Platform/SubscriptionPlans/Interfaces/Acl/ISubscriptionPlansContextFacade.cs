using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

/// <summary>
/// Entitlement contract for IoT Assets (device limit) and Dashboard Analytics (features). It performs no user
/// authorization and must never be replaced by a plan name supplied by a client.
/// </summary>
public interface ISubscriptionPlansContextFacade
{
    Task<RanchEntitlement> GetEntitlementAsync(Guid ranchId, CancellationToken cancellationToken);

    Task<bool> HasFeatureAsync(Guid ranchId, FeatureCode feature, CancellationToken cancellationToken);
}
