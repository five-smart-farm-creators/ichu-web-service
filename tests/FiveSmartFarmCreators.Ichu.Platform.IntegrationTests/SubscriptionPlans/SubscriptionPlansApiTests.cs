using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.SubscriptionPlans;

[Collection(ApiCollection.Name)]
public sealed class SubscriptionPlansApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task GetPlans_ReturnsSeededCatalogWithoutAuthentication()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("api/v1/plans");

        var plans = (await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray()
            .ToDictionary(plan => plan.GetProperty("name").GetString()!);
        Assert.Equal(3, plans.Count);
        Assert.Equal(50, plans["Basic"].GetProperty("deviceLimit").GetInt32());
        Assert.Equal(200, plans["Standard"].GetProperty("deviceLimit").GetInt32());
        Assert.Equal(500, plans["Pro"].GetProperty("deviceLimit").GetInt32());
        Assert.Contains("ReportExport", plans["Pro"].GetProperty("features").EnumerateArray().Select(feature => feature.GetString()));
        Assert.DoesNotContain("ReportExport", plans["Standard"].GetProperty("features").EnumerateArray().Select(feature => feature.GetString()));
    }

    [Fact]
    public async Task QuotePlan_AddsAnnualFeeToDeviceCostOfEachHead()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/plans/{Arrange.StandardPlanId}/quote?headCount=10");

        var quote = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(1800m, quote.GetProperty("annualFee").GetProperty("amount").GetDecimal());
        Assert.Equal(1200m, quote.GetProperty("devicesCost").GetProperty("amount").GetDecimal());
        Assert.Equal(3000m, quote.GetProperty("total").GetProperty("amount").GetDecimal());
        Assert.Equal("PEN", quote.GetProperty("total").GetProperty("currency").GetString());
    }

    [Fact]
    public async Task QuotePlan_WhenHeadCountExceedsDeviceLimit_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/plans/{Arrange.BasicPlanId}/quote?headCount=51");

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("SubscriptionPlans.HeadCountExceedsPlan", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task QuotePlan_ForUnknownPlan_ReturnsNotFound()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/plans/{Guid.NewGuid()}/quote?headCount=10");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("SubscriptionPlans.PlanNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RequestSubscription_CreatesPendingSubscriptionAtQuotedPrice()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var subscription = await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.StandardPlanId);

        Assert.Equal("Pending", subscription.GetProperty("status").GetString());
        Assert.Equal(ranchId, subscription.GetProperty("ranchId").GetGuid());
        Assert.Equal(200, subscription.GetProperty("maximumDevices").GetInt32());
        Assert.Equal(3000m, subscription.GetProperty("total").GetProperty("amount").GetDecimal());
        Assert.Equal(JsonValueKind.Null, subscription.GetProperty("validUntil").ValueKind);
    }

    [Fact]
    public async Task RequestSubscription_RetriedWithSameOperation_ReturnsStoredSubscription()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var request = SubscriptionRequest(ranchId, Arrange.StandardPlanId, operationId: Guid.NewGuid());
        using var created = await client.PostJsonAsync("api/v1/subscriptions", request);
        var first = await created.ReadJsonAsync(HttpStatusCode.Created);

        using var retried = await client.PostJsonAsync("api/v1/subscriptions", request);

        var second = await retried.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(first.GetProperty("id").GetGuid(), second.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task RequestSubscription_WhenRanchAlreadyHasCurrentSubscription_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.BasicPlanId);

        using var response = await client.PostJsonAsync("api/v1/subscriptions",
            SubscriptionRequest(ranchId, Arrange.StandardPlanId, Guid.NewGuid()));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("SubscriptionPlans.SubscriptionAlreadyExists", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RequestSubscription_WithOutdatedTerms_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync("api/v1/subscriptions",
            SubscriptionRequest(ranchId, Arrange.StandardPlanId, Guid.NewGuid(), termsVersion: "2020-01"));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("SubscriptionPlans.TermsVersionOutdated", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RequestSubscription_WithStaleQuote_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync("api/v1/subscriptions",
            SubscriptionRequest(ranchId, Arrange.StandardPlanId, Guid.NewGuid(), expectedTotal: 2999m));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("SubscriptionPlans.QuoteChanged", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetSubscription_BeforeActivation_ReturnsPendingSubscription()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var requested = await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.StandardPlanId);

        using var response = await client.GetAsync($"api/v1/subscriptions?ranchId={ranchId}");

        var subscription = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(requested.GetProperty("id").GetGuid(), subscription.GetProperty("id").GetGuid());
        Assert.Equal("Pending", subscription.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetSubscription_AfterActivation_IsActiveForOneYear()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);

        using var response = await client.GetAsync($"api/v1/subscriptions?ranchId={ranchId}");

        var subscription = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Active", subscription.GetProperty("status").GetString());
        var activatedAt = subscription.GetProperty("activatedAt").GetDateTimeOffset();
        Assert.Equal(activatedAt.AddYears(1), subscription.GetProperty("validUntil").GetDateTimeOffset());
    }

    [Fact]
    public async Task GetSubscription_ForAnotherRanch_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await Arrange.RequestSubscriptionAsync(owner, ranchId, Arrange.StandardPlanId);
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync($"api/v1/subscriptions?ranchId={ranchId}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("SubscriptionPlans.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CancelSubscription_WithReason_MarksSubscriptionCancelled()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var requested = await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.BasicPlanId);
        var subscriptionId = requested.GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/subscriptions/{subscriptionId}/cancellations", new { reason = "Ranch sold" });

        var subscription = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Cancelled", subscription.GetProperty("status").GetString());
        Assert.Equal("Ranch sold", subscription.GetProperty("cancellationReason").GetString());
    }

    [Fact]
    public async Task CancelSubscription_WithBlankReason_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var requested = await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.BasicPlanId);
        var subscriptionId = requested.GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/subscriptions/{subscriptionId}/cancellations", new { reason = " " });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("SubscriptionPlans.CancellationReasonRequired", problem.GetProperty("code").GetString());
    }

    private static object SubscriptionRequest(Guid ranchId, Guid planId, Guid operationId, string termsVersion = "2026-11", decimal expectedTotal = 3000m) =>
        new
        {
            ranchId,
            planId,
            headCount = 10,
            expectedTotal,
            termsVersion,
            operationId,
            acquisitionSource = "integration-test"
        };
}
