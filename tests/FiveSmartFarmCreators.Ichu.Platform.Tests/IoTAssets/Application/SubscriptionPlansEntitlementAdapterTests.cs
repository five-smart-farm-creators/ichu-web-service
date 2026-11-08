using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.IoTAssets.Application;

public class SubscriptionPlansEntitlementAdapterTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    [Fact]
    public async Task GetAsync_MapsTheDeviceLimitOfTheActivePlanToTheCollarLimit()
    {
        var plans = new FakeSubscriptionPlansFacade(new SubscriptionEntitlements(true, 200, ["Monitoring"]));
        var adapter = new SubscriptionPlansEntitlementAdapter(plans);

        var result = await adapter.GetAsync(RanchId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new CollarEntitlement(IsActive: true, MaximumCollars: 200), result.Value);
        Assert.Equal(RanchId, plans.LastRanchId);
    }

    [Fact]
    public async Task GetAsync_WithoutAnActiveSubscriptionAllowsNoCollars()
    {
        var adapter = new SubscriptionPlansEntitlementAdapter(
            new FakeSubscriptionPlansFacade(new SubscriptionEntitlements(false, 0, [])));

        var result = await adapter.GetAsync(RanchId, CancellationToken.None);

        Assert.False(result.Value!.IsActive);
        Assert.Equal(0, result.Value.MaximumCollars);
    }

    private sealed class FakeSubscriptionPlansFacade(SubscriptionEntitlements entitlements) : ISubscriptionPlansContextFacade
    {
        public Guid? LastRanchId { get; private set; }

        public Task<SubscriptionEntitlements> GetEntitlementsAsync(Guid ranchId, CancellationToken cancellationToken)
        {
            LastRanchId = ranchId;
            return Task.FromResult(entitlements);
        }

        public Task<bool> IncludesFeatureAsync(Guid ranchId, string featureCode, CancellationToken cancellationToken) =>
            Task.FromResult(entitlements.Features.Contains(featureCode));
    }
}
