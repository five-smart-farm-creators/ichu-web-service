namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Payment that the provider reported for a subscription, with its amount in minor currency units. It is trusted only
///     after the provider verified the signature of the event that carries it.
/// </summary>
public sealed record PaymentConfirmation(
    string EventId,
    string SessionId,
    SubscriptionId SubscriptionId,
    long AmountMinor,
    string Currency,
    bool Paid);
