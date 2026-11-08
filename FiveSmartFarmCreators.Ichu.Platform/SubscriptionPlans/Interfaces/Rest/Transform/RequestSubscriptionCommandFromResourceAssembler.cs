using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

public static class RequestSubscriptionCommandFromResourceAssembler
{
    public static RequestSubscriptionCommand ToCommandFromResource(RequestSubscriptionResource resource) =>
        new(
            resource.RanchId,
            new PlanId(resource.PlanId),
            resource.HeadCount,
            resource.ExpectedTotal,
            resource.TermsVersion,
            resource.OperationId,
            resource.AcquisitionSource);
}
