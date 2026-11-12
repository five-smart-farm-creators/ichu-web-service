using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

/// <summary>
///     Maps a subscription and its plan to the resource that clients read. The plan supplies the device limit and the
///     features, which the subscription itself does not store.
/// </summary>
public static class SubscriptionResourceFromEntityAssembler
{
    public static SubscriptionResource ToResourceFromEntity(Subscription subscription, Plan plan) =>
        new(
            subscription.Id.Value,
            subscription.RanchId,
            subscription.PlanId.Value,
            subscription.Status.ToString(),
            subscription.HeadCount.Value,
            new MoneyResource(subscription.AnnualFee.Amount, subscription.AnnualFee.Currency),
            new MoneyResource(subscription.DeviceUnitPrice.Amount, subscription.DeviceUnitPrice.Currency),
            new MoneyResource(subscription.DevicesCost.Amount, subscription.DevicesCost.Currency),
            new MoneyResource(subscription.Total.Amount, subscription.Total.Currency),
            subscription.TermsVersion,
            subscription.RequestedAt,
            subscription.ActivatedAt,
            subscription.ValidUntil,
            subscription.CancelledAt,
            subscription.CancellationReason,
            plan.DeviceLimit.Maximum,
            plan.Features.Select(feature => feature.ToString()).ToList());
}
