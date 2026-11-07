using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.SubscriptionPlans.Domain;

public class PlanTests
{
    [Fact]
    public void EstimateFor_AddsTheAnnualFeeToTheDevicesCost()
    {
        var quote = StandardPlan().EstimateFor(new HeadCount(120));

        Assert.True(quote.IsSuccess);
        Assert.Equal(14400m, quote.Value!.DevicesCost.Amount);
        Assert.Equal(16200m, quote.Value.Total.Amount);
        Assert.Equal("PEN", quote.Value.Total.Currency);
    }

    [Fact]
    public void EstimateFor_RejectsHeadCountAboveTheDeviceLimit()
    {
        var quote = BasicPlan().EstimateFor(new HeadCount(51));

        Assert.True(quote.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.HeadCountExceedsPlan.Code, quote.DomainError.Code);
    }

    [Fact]
    public void EstimateFor_RejectsUnpublishedPlan()
    {
        var plan = new Plan(new PlanId(Guid.NewGuid()), PlanTier.Basic, "Basic", new DeviceLimit(50),
            new Money(600m, "PEN"), new Money(120m, "PEN"), [FeatureCode.Monitoring], isPublished: false);

        var quote = plan.EstimateFor(new HeadCount(10));

        Assert.True(quote.IsFailure);
        Assert.Equal(SubscriptionPlansErrors.PlanUnavailable.Code, quote.DomainError.Code);
    }

    [Fact]
    public void Includes_ReportsOnlyTheFeaturesTheTierGrants()
    {
        var plan = StandardPlan();

        Assert.True(plan.Includes(FeatureCode.Analytics));
        Assert.False(plan.Includes(FeatureCode.ReportExport));
    }

    private static Plan StandardPlan() => new(new PlanId(Guid.NewGuid()), PlanTier.Standard, "Standard",
        new DeviceLimit(200), new Money(1800m, "PEN"), new Money(120m, "PEN"),
        [FeatureCode.Monitoring, FeatureCode.Alerts, FeatureCode.Planning, FeatureCode.Analytics], isPublished: true);

    private static Plan BasicPlan() => new(new PlanId(Guid.NewGuid()), PlanTier.Basic, "Basic",
        new DeviceLimit(50), new Money(600m, "PEN"), new Money(120m, "PEN"),
        [FeatureCode.Monitoring, FeatureCode.Alerts], isPublished: true);
}
