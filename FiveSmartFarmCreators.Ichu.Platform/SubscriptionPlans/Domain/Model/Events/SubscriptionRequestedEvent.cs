using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Events;

/// <summary>
///     Raised when a ranch requests a subscription, which stays pending until it is confirmed.
/// </summary>
public sealed record SubscriptionRequestedEvent(SubscriptionId SubscriptionId, Guid RanchId, PlanId PlanId, Money Total) : IEvent;
