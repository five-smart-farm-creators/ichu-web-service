using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Events;

/// <summary>
///     Raised when a pending subscription becomes active and grants coverage until its validity end.
/// </summary>
public sealed record SubscriptionActivatedEvent(SubscriptionId SubscriptionId, Guid RanchId, DateTimeOffset ValidUntil) : IEvent;
