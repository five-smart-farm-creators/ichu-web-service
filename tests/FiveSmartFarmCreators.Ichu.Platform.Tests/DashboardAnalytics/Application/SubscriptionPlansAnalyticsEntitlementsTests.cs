using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics.Application;

public class SubscriptionPlansAnalyticsEntitlementsTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    [Fact]
    public async Task CanExport_AsksThePlanForTheReportExportFeature()
    {
        var plans = new FakeSubscriptionPlans { Includes = true };

        var allowed = await new SubscriptionPlansAnalyticsEntitlements(plans).CanExportAsync(RanchId, CancellationToken.None);

        Assert.True(allowed);
        Assert.Equal((RanchId, "ReportExport"), Assert.Single(plans.Requests));
    }

    private sealed class FakeSubscriptionPlans : ISubscriptionPlansContextFacade
    {
        public bool Includes { get; init; }

        public List<(Guid RanchId, string FeatureCode)> Requests { get; } = [];

        public Task<SubscriptionEntitlements> GetEntitlementsAsync(Guid ranchId, CancellationToken cancellationToken) =>
            throw new NotSupportedException("The export gate asks for a single feature.");

        public Task<bool> IncludesFeatureAsync(Guid ranchId, string featureCode, CancellationToken cancellationToken)
        {
            Requests.Add((ranchId, featureCode));
            return Task.FromResult(Includes);
        }
    }
}
