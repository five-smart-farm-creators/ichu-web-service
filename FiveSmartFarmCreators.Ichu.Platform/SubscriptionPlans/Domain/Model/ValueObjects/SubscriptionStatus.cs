namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Lifecycle status of a subscription. Only a pending subscription can be activated, and only an active one expires.
/// </summary>
public enum SubscriptionStatus { Pending, Active, Cancelled, Expired }
