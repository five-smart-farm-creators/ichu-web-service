using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

public sealed class SubscriptionPlansContextFacade(
    ISubscriptionRepository subscriptions,
    IPlanRepository plans,
    TimeProvider clock) : ISubscriptionPlansContextFacade
{
    public async Task<RanchEntitlement> GetEntitlementAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.FindOpenByRanchAsync(ranchId, cancellationToken);
        if (subscription is null || !subscription.IsEntitledAt(clock.GetUtcNow()))
        {
            return RanchEntitlement.Inactive(ranchId);
        }

        var plan = await plans.FindAsync(subscription.PlanId, cancellationToken);
        if (plan is null)
        {
            return RanchEntitlement.Inactive(ranchId);
        }

        return new RanchEntitlement(
            ranchId,
            true,
            subscription.Id,
            plan.Id,
            plan.Code,
            plan.MaximumDevices,
            plan.Features.Select(f => f.Code).ToHashSet(),
            subscription.PaidUntil);
    }

    public async Task<bool> HasFeatureAsync(Guid ranchId, FeatureCode feature, CancellationToken cancellationToken) =>
        (await GetEntitlementAsync(ranchId, cancellationToken)).Features.Contains(feature);
}
