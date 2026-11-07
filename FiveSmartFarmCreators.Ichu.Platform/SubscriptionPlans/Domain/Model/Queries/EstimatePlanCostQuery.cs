using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;

/// <summary>
///     Quotes the annual price of a plan for a head count, before any subscription exists.
/// </summary>
public sealed record EstimatePlanCostQuery(PlanId PlanId, int HeadCount);
