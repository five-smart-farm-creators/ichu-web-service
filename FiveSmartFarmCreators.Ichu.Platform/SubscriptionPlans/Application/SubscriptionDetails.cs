using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>A subscription with the plan that defines its limits and features, evaluated at <see cref="EvaluatedAt"/>.</summary>
public sealed record SubscriptionDetails(Subscription Subscription, Plan Plan, DateTimeOffset EvaluatedAt);
