using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a subscription, distinct from the plan identifier it refers to.
/// </summary>
public readonly record struct SubscriptionId(Guid Value) : IEntityId<Guid>;
