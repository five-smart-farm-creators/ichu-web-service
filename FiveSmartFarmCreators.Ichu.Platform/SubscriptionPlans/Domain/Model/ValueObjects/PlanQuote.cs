namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

/// <summary>
/// A cost estimate, not a charge. The recurring amount is billed each period; the hardware amount is a separate
/// one-time cost. The quote identifier is stable for the same plan pricing version and head count.
/// </summary>
public sealed record PlanQuote(
    Guid QuoteId,
    Guid PlanId,
    int PricingVersion,
    int HeadCount,
    BillingPeriod BillingPeriod,
    Money PricePerHead,
    Money RecurringAmount,
    Money HardwareUnitPrice,
    Money HardwareAmount,
    DateTimeOffset ValidUntil);
