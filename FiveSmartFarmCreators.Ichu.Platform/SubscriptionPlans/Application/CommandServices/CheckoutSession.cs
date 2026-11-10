namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;

/// <summary>
///     Hosted payment page that a ranch manager opens to pay a subscription before the session expires.
/// </summary>
public sealed record CheckoutSession(string SessionId, Uri Url, DateTimeOffset ExpiresAt);
