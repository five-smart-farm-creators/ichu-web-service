using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Events;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Events;

/// <summary>
///     Raised when a subscription is cancelled, with the reason recorded for follow-up.
/// </summary>
public sealed record SubscriptionCancelledEvent(SubscriptionId SubscriptionId, Guid RanchId, string Reason) : IEvent;
