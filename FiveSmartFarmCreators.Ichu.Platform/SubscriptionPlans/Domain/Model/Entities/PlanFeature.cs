using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Entities;

public sealed class PlanFeature
{
    public FeatureCode Code { get; private set; }

    private PlanFeature()
    {
    }

    public PlanFeature(FeatureCode code)
    {
        Code = code;
    }
}
