using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

public static class CheckoutSessionResourceFromValueAssembler
{
    public static CheckoutSessionResource ToResourceFromValue(CheckoutSession session) =>
        new(session.SessionId, session.Url, session.ExpiresAt);
}
