namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>
///     Hosted payment page of a subscription. The client sends the ranch manager to the URL before the expiry instant.
/// </summary>
public sealed record CheckoutSessionResource(string SessionId, Uri Url, DateTimeOffset ExpiresAt);
