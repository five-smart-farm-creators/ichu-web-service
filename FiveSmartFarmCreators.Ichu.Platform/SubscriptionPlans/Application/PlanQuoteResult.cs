using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>A valid quote, or the coverage restriction with another plan only when its coverage admits the head count.</summary>
public sealed record PlanQuoteResult(PlanQuote? Quote, string? Restriction, Guid? SuggestedPlanId);
