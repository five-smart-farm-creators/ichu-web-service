using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

public static class PlanResourceFromEntityAssembler
{
    public static PlanResource ToResourceFromEntity(Plan plan) =>
        new(
            plan.Id.Value,
            plan.Tier.ToString(),
            plan.Name,
            plan.DeviceLimit.Maximum,
            new MoneyResource(plan.AnnualFee.Amount, plan.AnnualFee.Currency),
            new MoneyResource(plan.DeviceUnitPrice.Amount, plan.DeviceUnitPrice.Currency),
            plan.Features.Select(feature => feature.ToString()).ToList());
}
