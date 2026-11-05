namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>Created is false when the same operation was already registered and its existing request is returned.</summary>
public sealed record SubscriptionRequestResult(bool Created, SubscriptionDetails Details);
