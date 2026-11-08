using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Rest.Transform;

public static class PlanQuoteResourceFromValueAssembler
{
    public static PlanQuoteResource ToResourceFromValue(PlanQuote quote) =>
        new(
            quote.PlanId.Value,
            quote.HeadCount.Value,
            new MoneyResource(quote.AnnualFee.Amount, quote.AnnualFee.Currency),
            new MoneyResource(quote.DeviceUnitPrice.Amount, quote.DeviceUnitPrice.Currency),
            new MoneyResource(quote.DevicesCost.Amount, quote.DevicesCost.Currency),
            new MoneyResource(quote.Total.Amount, quote.Total.Currency));
}
