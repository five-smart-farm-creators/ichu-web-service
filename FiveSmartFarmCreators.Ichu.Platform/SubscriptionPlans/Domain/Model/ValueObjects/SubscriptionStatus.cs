namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
/// Pending, Active and Cancelled are stored. Expired is derived when an active subscription reaches the end of its
/// paid period without an accepted renewal.
/// </summary>
public enum SubscriptionStatus
{
    Pending,
    Active,
    Cancelled,
    Expired
}
