using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>
///     Starts the hosted checkout of a pending subscription that belongs to a ranch.
/// </summary>
public sealed record StartSubscriptionCheckoutCommand(Guid RanchId, SubscriptionId SubscriptionId);
