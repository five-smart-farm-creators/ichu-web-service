using System.Net;
using System.Net.Http.Headers;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.SubscriptionPlans;

/// <summary>
///     Starts the payment of a subscription through a fake provider, so that the tests never reach Stripe.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class SubscriptionCheckoutApiTests(IchuApiFactory factory) : IDisposable
{
    private static readonly DateTimeOffset SessionExpiry = new(2030, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // Only this host replaces the Stripe adapter, because the shared factory keeps the real adapter for the webhook tests.
    private readonly WebApplicationFactory<Program> _host = factory.WithWebHostBuilder(builder =>
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPaymentGateway>();
            services.AddSingleton<IPaymentGateway>(new FakePaymentGateway());
        }));

    [Fact]
    public async Task StartCheckout_ForPendingSubscription_ReturnsTheHostedSession()
    {
        using var client = CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(client, ranchId);

        using var response = await client.PostAsync(CheckoutPath(ranchId, subscriptionId), content: null);

        var session = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal("cs_test_fake", session.GetProperty("sessionId").GetString());
        Assert.Equal("https://checkout.example.test/cs_test_fake", session.GetProperty("url").GetString());
        Assert.Equal(SessionExpiry, session.GetProperty("expiresAt").GetDateTimeOffset());
        using var subscriptionResponse = await client.GetAsync($"api/v1/ranches/{ranchId}/subscriptions");
        var subscription = await subscriptionResponse.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Pending", subscription.GetProperty("status").GetString());
        Assert.Equal(1, Payments.CheckoutRequests);
    }

    [Fact]
    public async Task GetQuote_DoesNotCreateASubscriptionOrStartCheckout()
    {
        using var manager = CreateClient(TestIdentity.Create());
        var ranchId = await manager.RegisterManagerAsync();
        using var visitor = _host.CreateClient();

        using var response = await visitor.GetAsync($"api/v1/quotes?planId={Arrange.StandardPlanId}&headCount=10");

        await response.ReadJsonAsync(HttpStatusCode.OK);
        using var subscriptionResponse = await manager.GetAsync($"api/v1/ranches/{ranchId}/subscriptions");
        var problem = await subscriptionResponse.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("SubscriptionPlans.SubscriptionNotFound", problem.GetProperty("code").GetString());
        Assert.Equal(0, Payments.CheckoutRequests);
    }

    [Fact]
    public async Task StartCheckout_ForActiveSubscription_ReturnsConflict()
    {
        using var client = CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var subscriptionId = await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);

        using var response = await client.PostAsync(CheckoutPath(ranchId, subscriptionId), content: null);

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("SubscriptionPlans.SubscriptionNotPending", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task StartCheckout_ForSubscriptionOfAnotherRanchOfTheCaller_IsNotFound()
    {
        using var owner = CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(owner, ranchId);
        using var caller = CreateClient(TestIdentity.Create());
        var callerRanchId = await caller.RegisterManagerAsync();

        using var response = await caller.PostAsync(CheckoutPath(callerRanchId, subscriptionId), content: null);

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("SubscriptionPlans.SubscriptionNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task StartCheckout_ForRanchOfAnotherAccount_IsForbidden()
    {
        using var owner = CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(owner, ranchId);
        using var outsider = CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PostAsync(CheckoutPath(ranchId, subscriptionId), content: null);

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("SubscriptionPlans.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    public void Dispose() => _host.Dispose();

    private FakePaymentGateway Payments => (FakePaymentGateway)_host.Services.GetRequiredService<IPaymentGateway>();

    private HttpClient CreateClient(TestIdentity identity)
    {
        var client = _host.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", identity.BearerToken);
        return client;
    }

    private static async Task<Guid> RequestSubscriptionAsync(HttpClient client, Guid ranchId) =>
        (await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.StandardPlanId)).GetProperty("id").GetGuid();

    private static string CheckoutPath(Guid ranchId, Guid subscriptionId) =>
        $"api/v1/ranches/{ranchId}/subscriptions/{subscriptionId}/checkout-sessions";

    private sealed class FakePaymentGateway : IPaymentGateway
    {
        public int CheckoutRequests { get; private set; }

        public Task<Result<CheckoutSession>> CreateCheckoutSessionAsync(Subscription subscription, string planName, CancellationToken cancellationToken)
        {
            CheckoutRequests++;
            return Task.FromResult(Result<CheckoutSession>.Success(new CheckoutSession(
                "cs_test_fake", new Uri("https://checkout.example.test/cs_test_fake"), SessionExpiry)));
        }

        public Result<PaymentConfirmation> ReadConfirmation(string payload, string signatureHeader) =>
            throw new NotSupportedException("The checkout tests do not receive payment events.");
    }
}
