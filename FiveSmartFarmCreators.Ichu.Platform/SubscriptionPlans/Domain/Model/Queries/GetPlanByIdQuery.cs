using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;

/// <summary>
///     Reads one plan, whether or not it is currently sold, because existing subscriptions still refer to it.
/// </summary>
public sealed record GetPlanByIdQuery(PlanId PlanId);
