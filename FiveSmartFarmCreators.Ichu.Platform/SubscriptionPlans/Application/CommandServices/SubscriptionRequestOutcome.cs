using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;

/// <summary>
///     Outcome of a subscription request. A repeated operation returns the stored subscription with <c>Created</c> false.
/// </summary>
public sealed record SubscriptionRequestOutcome(Subscription Subscription, bool Created);
