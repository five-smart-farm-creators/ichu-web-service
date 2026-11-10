using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;

/// <summary>
///     Outbound port to the payment provider. Provider failures are returned as Subscription Plans error codes, so no
///     provider exception reaches the application layer.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    ///     Creates a hosted checkout that charges the total of a subscription. The provider reports the payment back through
    ///     the webhook.
    /// </summary>
    Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(Subscription subscription, string planName, CancellationToken cancellationToken);

    /// <summary>
    ///     Verifies the signature of a provider event and reads the payment it reports. Events that are not payments of a
    ///     checkout are reported as ignored.
    /// </summary>
    Result<PaymentConfirmation> ReadConfirmation(string payload, string signatureHeader);
}
