using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
///     Strongly typed identifier of a subscription plan, so that plan and subscription identifiers cannot be mixed up.
/// </summary>
public readonly record struct PlanId(Guid Value) : IEntityId<Guid>;
