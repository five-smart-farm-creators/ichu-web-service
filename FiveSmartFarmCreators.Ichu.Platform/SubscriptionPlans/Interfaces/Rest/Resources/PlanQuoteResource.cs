using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

/// <summary>An estimate valid until ValidUntil. IsEstimate is always true: a quote is not a charge or a payment.</summary>
public sealed record PlanQuoteResource(
    Guid QuoteId,
    Guid PlanId,
    int HeadCount,
    string BillingPeriod,
    MoneyResource PricePerHead,
    MoneyResource RecurringAmount,
    MoneyResource HardwareUnitPrice,
    MoneyResource HardwareAmount,
    DateTimeOffset ValidUntil,
    bool IsEstimate)
{
    public static PlanQuoteResource From(PlanQuote quote) =>
        new(
            quote.QuoteId,
            quote.PlanId,
            quote.HeadCount,
            quote.BillingPeriod.ToString(),
            MoneyResource.From(quote.PricePerHead),
            MoneyResource.From(quote.RecurringAmount),
            MoneyResource.From(quote.HardwareUnitPrice),
            MoneyResource.From(quote.HardwareAmount),
            quote.ValidUntil,
            true);
}
