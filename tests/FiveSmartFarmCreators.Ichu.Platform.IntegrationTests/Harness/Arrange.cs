using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

/// <summary>
///     Creates the records that a scenario needs before the behavior under test runs.
/// </summary>
public static class Arrange
{
    public static readonly Guid BasicPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000001");
    public static readonly Guid StandardPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000002");
    public static readonly Guid ProPlanId = Guid.Parse("5b1f0d2e-0c11-4a51-9b01-000000000003");

    public static async Task<JsonElement> RequestSubscriptionAsync(HttpClient client, Guid ranchId, Guid planId, int headCount = 10)
    {
        using var quote = await client.GetAsync($"api/v1/plans/{planId}/quote?headCount={headCount}");
        var quoted = await quote.ReadJsonAsync(HttpStatusCode.OK);
        using var response = await client.PostJsonAsync("api/v1/subscriptions", new
        {
            ranchId,
            planId,
            headCount,
            expectedTotal = quoted.GetProperty("total").GetProperty("amount").GetDecimal(),
            termsVersion = "2026-11",
            operationId = Guid.NewGuid(),
            acquisitionSource = "integration-test"
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    /// <summary>
    ///     Requests a subscription and activates it, returning the subscription identifier.
    /// </summary>
    public static async Task<Guid> ActivateSubscriptionAsync(IchuApiFactory factory, HttpClient client, Guid ranchId, Guid planId)
    {
        var subscription = await RequestSubscriptionAsync(client, ranchId, planId);
        var subscriptionId = subscription.GetProperty("id").GetGuid();

        // Payment confirmation has no HTTP endpoint, so the test activates the subscription the way the development seeder does.
        using var scope = factory.Services.CreateScope();
        var commands = scope.ServiceProvider.GetRequiredService<ISubscriptionCommandService>();
        var activation = await commands.Handle(new ActivateSubscriptionCommand(new SubscriptionId(subscriptionId)), CancellationToken.None);
        Assert.True(activation.IsSuccess, activation.Message);
        return subscriptionId;
    }

    public static async Task<JsonElement> RegisterCattleAsync(HttpClient client, Guid ranchId, string earTag, int lifeStage = 2)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/cattle", new
        {
            earTag,
            breed = 1,
            sex = 1,
            birthDate = new DateOnly(2024, 3, 1),
            lifeStage
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    public static async Task<JsonElement> CreateLotAsync(HttpClient client, Guid ranchId, string name)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/lots", new { name, criterion = "Weight" });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    public static async Task<JsonElement> RegisterCollarAsync(HttpClient client, Guid ranchId, string serial)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices", new
        {
            serial,
            type = 1,
            firmwareVersion = "1.0.0"
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    public static async Task<JsonElement> AssignCollarAsync(HttpClient client, Guid ranchId, Guid deviceId, Guid cattleId, Guid deviceVersion)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices/{deviceId}/assignments", new
        {
            targetId = cattleId,
            expectedVersion = deviceVersion
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    public static async Task<JsonElement> RegisterGatewayAsync(HttpClient client, Guid ranchId, string name)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/edge-gateways", new { name });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    public static async Task<JsonElement> AssignToLotAsync(HttpClient client, Guid ranchId, Guid lotId, Guid cattleId, Guid lotVersion)
    {
        using var response = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/cattle/{cattleId}", new
        {
            expectedVersion = lotVersion
        });
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    /// <summary>
    ///     Configures the body-temperature threshold of a ranch used by the fever alert scenarios.
    /// </summary>
    public static async Task<JsonElement> ConfigureFeverThresholdAsync(HttpClient client, Guid ranchId)
    {
        using var response = await client.PutJsonAsync("api/v1/monitoring-thresholds", new
        {
            ranchId,
            lifeStage = "MilkingCow",
            metric = "BodyTemperature",
            minValue = 37.5,
            maxValue = 39.5,
            severity = "Warning"
        });
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    public static async Task<JsonElement> SynchronizeReadingAsync(HttpClient client, string gatewayKey, Guid deviceId, Guid readingId,
        double temperature, DateTimeOffset recordedAt)
    {
        using var response = await client.PostJsonWithGatewayKeyAsync("api/v1/telemetry/batch", gatewayKey, new
        {
            readings = new[] { ReadingEntry(deviceId, readingId, temperature, recordedAt) }
        });
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    /// <summary>
    ///     Sends one fever reading and a temperature alert that cites it, as an edge gateway does.
    /// </summary>
    public static async Task<JsonElement> SynchronizeFeverAsync(HttpClient client, string gatewayKey, Guid deviceId, Guid readingId,
        Guid alertId, double temperature, DateTimeOffset recordedAt)
    {
        using var response = await client.PostJsonWithGatewayKeyAsync("api/v1/telemetry/batch", gatewayKey, new
        {
            readings = new[] { ReadingEntry(deviceId, readingId, temperature, recordedAt) },
            alerts = new[]
            {
                new
                {
                    alertId,
                    deviceId,
                    type = "Temperature",
                    severity = "Warning",
                    readingIds = new[] { readingId },
                    configurationRevision = 1,
                    thresholdMinValue = 37.5,
                    thresholdMaxValue = 39.5
                }
            }
        });
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }

    private static object ReadingEntry(Guid deviceId, Guid readingId, double temperature, DateTimeOffset recordedAt) => new
    {
        readingId,
        deviceId,
        recordedAt,
        bodyTemperature = new { value = temperature }
    };
}
