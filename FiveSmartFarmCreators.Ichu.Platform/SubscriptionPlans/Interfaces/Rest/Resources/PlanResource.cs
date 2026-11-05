using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>A published plan. A null maximum head count means the coverage has no upper limit.</summary>
public sealed record PlanResource(
    Guid PlanId,
    string Code,
    string Name,
    string BillingPeriod,
    MoneyResource PricePerHead,
    MoneyResource HardwareUnitPrice,
    int MinimumHeadCount,
    int? MaximumHeadCount,
    int MaximumDevices,
    IReadOnlyList<string> Features,
    DateTimeOffset PricesValidUntil)
{
    public static PlanResource From(Plan plan) =>
        new(
            plan.Id,
            plan.Code,
            plan.Name,
            plan.BillingPeriod.ToString(),
            MoneyResource.From(plan.PricePerHead),
            MoneyResource.From(plan.HardwareUnitPrice),
            plan.MinimumHeadCount,
            plan.MaximumHeadCount,
            plan.MaximumDevices,
            plan.Features.Select(f => f.Code.ToString()).Order(StringComparer.Ordinal).ToList(),
            plan.PricesValidUntil);
}
