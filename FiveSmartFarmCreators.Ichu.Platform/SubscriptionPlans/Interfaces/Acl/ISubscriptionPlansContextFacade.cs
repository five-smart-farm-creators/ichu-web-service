namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

/// <summary>
///     Exposes the entitlements of a ranch to other bounded contexts. Callers authorize the ranch themselves; this
///     contract only reports what the current plan grants, and a ranch without an active subscription gets nothing.
/// </summary>
public interface ISubscriptionPlansContextFacade
{
    Task<SubscriptionEntitlements> GetEntitlementsAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    ///     Determines whether the active plan of a ranch grants a feature named as in <c>FeatureCode</c>, such as Analytics.
    /// </summary>
    Task<bool> IncludesFeatureAsync(Guid ranchId, string featureCode, CancellationToken cancellationToken);
}
