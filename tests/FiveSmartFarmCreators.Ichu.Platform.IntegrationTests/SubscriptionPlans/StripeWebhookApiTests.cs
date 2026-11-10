using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.SubscriptionPlans;

/// <summary>
///     Delivers signed Stripe events to the webhook, which runs the real Stripe adapter configured with the test secret.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class StripeWebhookApiTests(IchuApiFactory factory)
{
    private const string WebhookPath = "api/v1/payments/stripe/webhooks";
    private const string SignatureHeader = "Stripe-Signature";
    private const long StandardTotalMinor = 300000; // Ten head on the Standard plan cost 3000.00 PEN.

    [Fact]
    public async Task CompletedCheckout_ActivatesThePendingSubscription()
    {
        using var manager = factory.CreateClient(TestIdentity.Create());
        var ranchId = await manager.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(manager, ranchId);
        using var webhook = factory.CreateClient();

        using var response = await PostEventAsync(webhook,
            CheckoutEvent("checkout.session.completed", subscriptionId, StandardTotalMinor, "paid"), IchuApiFactory.StripeWebhookSecret);

        await response.ReadBodyAsync(HttpStatusCode.OK);
        Assert.Equal("Active", (await ReadSubscriptionAsync(manager, ranchId)).GetProperty("status").GetString());
    }

    [Fact]
    public async Task RepeatedDelivery_KeepsTheActivationUnchanged()
    {
        using var manager = factory.CreateClient(TestIdentity.Create());
        var ranchId = await manager.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(manager, ranchId);
        using var webhook = factory.CreateClient();
        var payload = CheckoutEvent("checkout.session.completed", subscriptionId, StandardTotalMinor, "paid");
        using var first = await PostEventAsync(webhook, payload, IchuApiFactory.StripeWebhookSecret);
        await first.ReadBodyAsync(HttpStatusCode.OK);
        var activatedAt = (await ReadSubscriptionAsync(manager, ranchId)).GetProperty("activatedAt").GetDateTimeOffset();

        using var second = await PostEventAsync(webhook, payload, IchuApiFactory.StripeWebhookSecret);

        await second.ReadBodyAsync(HttpStatusCode.OK);
        var subscription = await ReadSubscriptionAsync(manager, ranchId);
        Assert.Equal("Active", subscription.GetProperty("status").GetString());
        Assert.Equal(activatedAt, subscription.GetProperty("activatedAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task EventWithInvalidSignature_IsRefusedAndLeavesTheSubscriptionPending()
    {
        using var manager = factory.CreateClient(TestIdentity.Create());
        var ranchId = await manager.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(manager, ranchId);
        using var webhook = factory.CreateClient();

        using var response = await PostEventAsync(webhook,
            CheckoutEvent("checkout.session.completed", subscriptionId, StandardTotalMinor, "paid"), "whsec_signed_with_another_secret");

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("SubscriptionPlans.PaymentSignatureInvalid", problem.GetProperty("code").GetString());
        Assert.Equal("Pending", (await ReadSubscriptionAsync(manager, ranchId)).GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("checkout.session.completed", StandardTotalMinor - 1, "paid")] // Paid below the total.
    [InlineData("checkout.session.completed", StandardTotalMinor, "unpaid")] // The payment is still pending.
    [InlineData("checkout.session.expired", StandardTotalMinor, "unpaid")] // The checkout closed without a payment.
    public async Task EventThatDoesNotActivate_IsAcknowledgedAndLeavesTheSubscriptionPending(string type, long amountTotal, string paymentStatus)
    {
        using var manager = factory.CreateClient(TestIdentity.Create());
        var ranchId = await manager.RegisterManagerAsync();
        var subscriptionId = await RequestSubscriptionAsync(manager, ranchId);
        using var webhook = factory.CreateClient();

        using var response = await PostEventAsync(webhook, CheckoutEvent(type, subscriptionId, amountTotal, paymentStatus),
            IchuApiFactory.StripeWebhookSecret);

        await response.ReadBodyAsync(HttpStatusCode.OK);
        Assert.Equal("Pending", (await ReadSubscriptionAsync(manager, ranchId)).GetProperty("status").GetString());
    }

    private static async Task<Guid> RequestSubscriptionAsync(HttpClient client, Guid ranchId) =>
        (await Arrange.RequestSubscriptionAsync(client, ranchId, Arrange.StandardPlanId)).GetProperty("id").GetGuid();

    private static async Task<JsonElement> ReadSubscriptionAsync(HttpClient manager, Guid ranchId)
    {
        using var response = await manager.GetAsync($"api/v1/ranches/{ranchId}/subscriptions");
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    private static async Task<HttpResponseMessage> PostEventAsync(HttpClient webhook, string payload, string secret)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, WebhookPath)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Add(SignatureHeader, Sign(payload, secret, DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
        return await webhook.SendAsync(request);
    }

    // Stripe signs "<timestamp>.<payload>" with HMAC-SHA256 keyed by the endpoint secret.
    private static string Sign(string payload, string secret, long timestamp)
    {
        var digest = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        return $"t={timestamp},v1={Convert.ToHexStringLower(digest)}";
    }

    private static string CheckoutEvent(string type, Guid subscriptionId, long amountTotal, string paymentStatus) => new JsonObject
    {
        ["id"] = $"evt_{Guid.NewGuid():N}",
        ["object"] = "event",
        ["type"] = type,
        ["data"] = new JsonObject
        {
            ["object"] = new JsonObject
            {
                ["id"] = $"cs_test_{Guid.NewGuid():N}",
                ["object"] = "checkout.session",
                ["mode"] = "payment",
                ["client_reference_id"] = subscriptionId.ToString(),
                ["amount_total"] = amountTotal,
                ["currency"] = "pen",
                ["payment_status"] = paymentStatus,
                ["metadata"] = new JsonObject { ["subscription_id"] = subscriptionId.ToString() }
            }
        }
    }.ToJsonString();
}
