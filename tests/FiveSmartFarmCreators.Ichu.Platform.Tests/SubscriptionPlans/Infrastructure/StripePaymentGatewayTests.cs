using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Payments.Stripe.Services;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Infrastructure;

public class StripePaymentGatewayTests
{
    [Fact]
    public void LineItemsFor_ChargesOneCollarPerHeadPlusTheAnnualFee()
    {
        var subscription = Request(annualFee: 1800m, deviceUnitPrice: 120m, headCount: 10);

        var items = StripePaymentGateway.LineItemsFor(subscription, "Basic");

        Assert.Collection(items,
            collars =>
            {
                Assert.Equal("ICHU smart collar", collars.PriceData.ProductData.Name);
                Assert.Equal(10, collars.Quantity);
                Assert.Equal(12000, collars.PriceData.UnitAmount);
            },
            fee =>
            {
                Assert.Equal("ICHU Basic plan — annual fee", fee.PriceData.ProductData.Name);
                Assert.Equal(1, fee.Quantity);
                Assert.Equal(180000, fee.PriceData.UnitAmount);
            });
        Assert.Equal(subscription.Total.ToMinorUnits(), items.Sum(item => item.Quantity!.Value * item.PriceData.UnitAmount!.Value));
    }

    [Fact]
    public void LineItemsFor_LeavesOutZeroPricedLines()
    {
        var subscription = Request(annualFee: 0m, deviceUnitPrice: 120m, headCount: 3);

        var item = Assert.Single(StripePaymentGateway.LineItemsFor(subscription, "Basic"));

        Assert.Equal("ICHU smart collar", item.PriceData.ProductData.Name);
    }

    private static Subscription Request(decimal annualFee, decimal deviceUnitPrice, int headCount)
    {
        var quote = new Plan(new PlanId(Guid.NewGuid()), PlanTier.Basic, "Basic", new DeviceLimit(50),
                new Money(annualFee, "PEN"), new Money(deviceUnitPrice, "PEN"), [FeatureCode.Monitoring], isPublished: true)
            .EstimateFor(new HeadCount(headCount))
            .Value!;
        return Subscription.Request(Guid.NewGuid(), quote, "2026-11", Guid.NewGuid(), null, DateTimeOffset.UnixEpoch);
    }
}
