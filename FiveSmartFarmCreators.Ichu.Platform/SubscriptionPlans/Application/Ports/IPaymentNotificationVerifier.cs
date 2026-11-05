namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;

/// <summary>Anti-corruption port towards the payment provider, which has not been selected yet.</summary>
public interface IPaymentNotificationVerifier
{
    /// <summary>
    /// Verifies the provider signature and translates the notification, or returns null when it cannot be
    /// authenticated. Unverified notifications must never change a payment or a subscription.
    /// </summary>
    Task<VerifiedPaymentResult?> VerifyAsync(string payload, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken);
}
