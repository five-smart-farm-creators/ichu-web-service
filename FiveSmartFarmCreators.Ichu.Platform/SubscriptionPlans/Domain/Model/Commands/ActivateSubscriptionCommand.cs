using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;

/// <summary>
///     Activates a pending subscription after a verified commercial confirmation. It is never exposed over HTTP.
/// </summary>
public sealed record ActivateSubscriptionCommand(SubscriptionId SubscriptionId);
