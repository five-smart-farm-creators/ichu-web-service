using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Acl;

public class SubscriptionPlansContextFacade(
    ISubscriptionRepository subscriptions,
    IPlanRepository plans,
    TimeProvider clock) : ISubscriptionPlansContextFacade
{
    private static readonly SubscriptionEntitlements NoEntitlements = new(false, 0, []);

    /// <inheritdoc />
    public async Task<SubscriptionEntitlements> GetEntitlementsAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var plan = await FindActivePlanAsync(ranchId, cancellationToken);
        return plan is null
            ? NoEntitlements
            : new SubscriptionEntitlements(true, plan.DeviceLimit.Maximum,
                plan.Features.Select(feature => feature.ToString()).ToList());
    }

    /// <inheritdoc />
    public async Task<bool> IncludesFeatureAsync(Guid ranchId, string featureCode, CancellationToken cancellationToken)
    {
        var entitlements = await GetEntitlementsAsync(ranchId, cancellationToken);
        return entitlements.Features.Contains(featureCode, StringComparer.Ordinal);
    }

    private async Task<Plan?> FindActivePlanAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.FindCurrentByRanchAsync(ranchId, cancellationToken);
        if (subscription is null || !subscription.IsActiveAt(clock.GetUtcNow()))
        {
            return null;
        }

        return await plans.FindByIdAsync(subscription.PlanId, cancellationToken);
    }
}
