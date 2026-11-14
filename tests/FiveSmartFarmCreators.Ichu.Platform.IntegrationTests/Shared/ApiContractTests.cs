using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Microsoft.Extensions.Configuration;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Shared;

[Collection(ApiCollection.Name)]
public sealed class ApiContractTests(IchuApiFactory factory)
{
    [Theory]
    [InlineData("get", "/api/v1/plans")]
    [InlineData("get", "/api/v1/quotes")]
    [InlineData("post", "/api/v1/accounts/sessions")]
    [InlineData("post", "/api/v1/telemetry/batch")]
    [InlineData("post", "/api/v1/water-controllers/{controllerId}/readings")]
    [InlineData("post", "/api/v1/payments/stripe/webhooks")]
    [InlineData("get", "/api/v1/edge-gateways/{gatewayId}/monitoring-rules")]
    [InlineData("get", "/api/v1/edge-gateways/{gatewayId}/device-assignments")]
    public async Task OpenApi_AnonymousOperations_DoNotRequireAUserBearerToken(string method, string path)
    {
        var document = await DocumentAsync();

        var operation = Operation(document, method, path);

        Assert.Empty(operation.GetProperty("security").EnumerateArray());
    }

    [Theory]
    [InlineData("get", "/api/v1/quotes", "200", false)]
    [InlineData("post", "/api/v1/ranches/{ranchId}/subscriptions", "201", true)]
    [InlineData("post", "/api/v1/ranches/{ranchId}/subscriptions/{subscriptionId}/checkout-sessions", "201", false)]
    [InlineData("post", "/api/v1/payments/stripe/webhooks", "200", true)]
    [InlineData("post", "/api/v1/veterinary-access-requests", "201", true)]
    [InlineData("patch", "/api/v1/veterinary-access-requests/{requestId}", "200", true)]
    [InlineData("get", "/api/v1/veterinary-access-requests", "200", false)]
    [InlineData("get", "/api/v1/ranches/{ranchId}/calendar", "200", false)]
    [InlineData("patch", "/api/v1/cattle/{cattleId}", "200", true)]
    [InlineData("post", "/api/v1/mobile-sync/batch", "200", true)]
    [InlineData("post", "/api/v1/telemetry/batch", "200", true)]
    [InlineData("post", "/api/v1/water-controllers/{controllerId}/readings", "201", true)]
    [InlineData("get", "/api/v1/water-controllers/{controllerId}/readings", "200", false)]
    [InlineData("get", "/api/v1/ranches/{ranchId}/water-troughs/status", "200", false)]
    [InlineData("get", "/api/v1/ranches/{ranchId}/water-troughs/{waterTroughId}/status", "200", false)]
    public async Task OpenApi_ReviewedFlows_DescribePurposeInputsAndResults(string method, string path, string successStatus, bool hasBody)
    {
        var document = await DocumentAsync();

        var operation = Operation(document, method, path);

        Assert.False(string.IsNullOrWhiteSpace(operation.GetProperty("summary").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(operation.GetProperty("description").GetString()));
        Assert.True(operation.GetProperty("responses").TryGetProperty(successStatus, out _));
        if (hasBody)
        {
            Assert.True(operation.GetProperty("requestBody").GetProperty("required").GetBoolean());
            Assert.True(operation.GetProperty("requestBody").GetProperty("content").TryGetProperty("application/json", out _));
        }
    }

    [Theory]
    [InlineData("get", "/api/v1/plans/{planId}/quote")]
    [InlineData("post", "/api/v1/ranches/{ranchId}/herd-advisories")]
    [InlineData("get", "/api/v1/ranches/{ranchId}/herd-advisories")]
    [InlineData("patch", "/api/v1/ranches/{ranchId}/herd-advisories/{advisoryId}")]
    [InlineData("put", "/api/v1/ranches/{ranchId}/cattle/{cattleId}")]
    [InlineData("patch", "/api/v1/ranches/{ranchId}/cattle/{cattleId}/life-stage")]
    public async Task OpenApi_CompatibilityOperations_AreDeprecatedWithTheReplacementDocumented(string method, string path)
    {
        var document = await DocumentAsync();

        var operation = Operation(document, method, path);

        Assert.True(operation.GetProperty("deprecated").GetBoolean());
        Assert.Contains("/api/v1/", operation.GetProperty("description").GetString());
    }

    [Fact]
    public async Task OpenApi_CalendarAndQuote_DeclareTheirRequiredQueryParameters()
    {
        var document = await DocumentAsync();
        var calendar = Operation(document, "get", "/api/v1/ranches/{ranchId}/calendar");
        var quote = Operation(document, "get", "/api/v1/quotes");

        foreach (var name in new[] { "from", "to" })
        {
            var parameter = Assert.Single(calendar.GetProperty("parameters").EnumerateArray(),
                parameter => parameter.GetProperty("name").GetString() == name);
            Assert.True(parameter.GetProperty("required").GetBoolean());
        }

        foreach (var name in new[] { "planId", "headCount" })
        {
            var parameter = Assert.Single(quote.GetProperty("parameters").EnumerateArray(),
                parameter => parameter.GetProperty("name").GetString() == name);
            Assert.True(parameter.GetProperty("required").GetBoolean());
        }
    }

    [Fact]
    public async Task OpenApi_StripeWebhook_DeclaresItsSignatureAndOriginalJsonBody()
    {
        var document = await DocumentAsync();
        var operation = Operation(document, "post", "/api/v1/payments/stripe/webhooks");

        var signature = Assert.Single(operation.GetProperty("parameters").EnumerateArray(),
            parameter => parameter.GetProperty("name").GetString() == "Stripe-Signature");

        Assert.Equal("header", signature.GetProperty("in").GetString());
        Assert.True(signature.GetProperty("required").GetBoolean());
        Assert.Contains("original Stripe event JSON", operation.GetProperty("requestBody").GetProperty("description").GetString());
    }

    [Fact]
    public async Task OpenApi_UserOperations_StillRequireTheBearerScheme()
    {
        var document = await DocumentAsync();

        Assert.Contains(document.GetProperty("security").EnumerateArray(), requirement => requirement.TryGetProperty("Bearer", out _));
        var operation = Operation(document, "post", "/api/v1/mobile-sync/batch");
        Assert.False(operation.TryGetProperty("security", out var security) && security.GetArrayLength() == 0);
    }

    [Fact]
    public async Task OpenApi_ExportsTheReviewedContractWhenASnapshotPathIsProvided()
    {
        var document = await DocumentAsync();
        Assert.True(document.GetProperty("paths").TryGetProperty("/api/v1/cattle/{cattleId}", out _));
        var snapshotPath = Environment.GetEnvironmentVariable("ICHU_OPENAPI_SNAPSHOT");
        if (!string.IsNullOrWhiteSpace(snapshotPath))
        {
            await File.WriteAllTextAsync(snapshotPath, document.GetRawText());
        }
    }

    [Fact]
    public async Task OpenApi_WaterReadingDocumentsRequiredFieldsGatewayHeaderAndUserQueryBounds()
    {
        var document = await DocumentAsync();
        var receive = Operation(document, "post", "/api/v1/water-controllers/{controllerId}/readings");
        var gatewayKey = Assert.Single(receive.GetProperty("parameters").EnumerateArray(),
            parameter => parameter.GetProperty("name").GetString() == "X-Gateway-Key");
        Assert.Equal("header", gatewayKey.GetProperty("in").GetString());
        Assert.True(gatewayKey.GetProperty("required").GetBoolean());
        Assert.Empty(receive.GetProperty("security").EnumerateArray());

        var schemaReference = receive.GetProperty("requestBody").GetProperty("content")
            .GetProperty("application/json").GetProperty("schema").GetProperty("$ref").GetString()!;
        var schemaName = schemaReference[(schemaReference.LastIndexOf('/') + 1)..];
        var required = document.GetProperty("components").GetProperty("schemas").GetProperty(schemaName)
            .GetProperty("required").EnumerateArray().Select(value => value.GetString()).ToHashSet();
        Assert.Contains("readingId", required);
        Assert.Contains("temperature", required);
        Assert.Contains("temperatureUnit", required);
        Assert.Contains("isValid", required);
        Assert.Contains("heaterState", required);
        Assert.Contains("capturedAt", required);
        Assert.DoesNotContain("events", required);

        var history = Operation(document, "get", "/api/v1/water-controllers/{controllerId}/readings");
        foreach (var name in new[] { "from", "to" })
        {
            var bound = Assert.Single(history.GetProperty("parameters").EnumerateArray(),
                parameter => parameter.GetProperty("name").GetString() == name);
            Assert.True(bound.GetProperty("required").GetBoolean());
        }
        Assert.False(history.TryGetProperty("security", out var security) && security.GetArrayLength() == 0);
    }

    private async Task<JsonElement> DocumentAsync()
    {
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["OpenApi:Enabled"] = "true" })));
        using var client = host.CreateClient();
        using var response = await client.GetAsync("openapi/v1.json");
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    private static JsonElement Operation(JsonElement document, string method, string path) =>
        document.GetProperty("paths").GetProperty(path).GetProperty(method);
}
