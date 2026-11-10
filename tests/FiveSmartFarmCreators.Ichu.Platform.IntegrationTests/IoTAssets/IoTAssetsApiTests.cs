using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.IoTAssets;

[Collection(ApiCollection.Name)]
public sealed class IoTAssetsApiTests(IchuApiFactory factory)
{

    [Fact]
    public async Task GetDeviceInventory_ForNewRanch_ReturnsEmptyList()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/devices");

        var devices = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Empty(devices.EnumerateArray());
    }

    [Fact]
    public async Task GetDevice_WithUnknownIdentifier_ReturnsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/devices/{Guid.NewGuid()}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("IoTAssets.DeviceNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetDeviceInventory_ByManagerOfAnotherRanch_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync($"api/v1/ranches/{ranchId}/devices");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("IoTAssets.Forbidden", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task AssignCollar_WithUnknownDevice_ReturnsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices/{Guid.NewGuid()}/assignments", new
        {
            targetId = Guid.NewGuid(),
            expectedVersion = Guid.NewGuid()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("IoTAssets.DeviceNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RevokeEdgeGateway_WithUnknownIdentifier_ReturnsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostAsync($"api/v1/ranches/{ranchId}/edge-gateways/{Guid.NewGuid()}/revocations", content: null);

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("IoTAssets.EdgeGatewayNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RegisterCollar_ReturnsAvailableCollarWithoutCommunication()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var collar = await Arrange.RegisterCollarAsync(client, ranchId, SerialNumber());

        Assert.Equal("SmartCollar", collar.GetProperty("type").GetString());
        Assert.Equal("Available", collar.GetProperty("status").GetString());
        Assert.True(collar.GetProperty("isAvailable").GetBoolean());
        Assert.Equal("NeverReported", collar.GetProperty("communicationStatus").GetString());
        Assert.Equal(JsonValueKind.Null, collar.GetProperty("currentAssignment").ValueKind);
    }

    [Fact]
    public async Task RegisterCollar_WithSerialAlreadyRegistered_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var serial = SerialNumber();
        await Arrange.RegisterCollarAsync(client, ranchId, serial);

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices", new { serial, type = 1 });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("IoTAssets.DuplicateSerial", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task AssignCollar_ToCattle_ReturnsInServiceDeviceWithCurrentAssignment()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0100");
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, SerialNumber());

        var assigned = await Arrange.AssignCollarAsync(client, ranchId, collar.GetProperty("id").GetGuid(),
            cattle.GetProperty("id").GetGuid(), collar.GetProperty("version").GetGuid());

        Assert.Equal("InService", assigned.GetProperty("status").GetString());
        Assert.False(assigned.GetProperty("isAvailable").GetBoolean());
        var current = assigned.GetProperty("currentAssignment");
        Assert.Equal(cattle.GetProperty("id").GetGuid(), current.GetProperty("targetId").GetGuid());
        Assert.Equal("Cattle", current.GetProperty("targetType").GetString());
        Assert.Equal(JsonValueKind.Null, current.GetProperty("endedAt").ValueKind);
    }

    [Fact]
    public async Task ReleaseCollar_ClosesAssignmentAndKeepsHistory()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0101");
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, SerialNumber());
        var deviceId = collar.GetProperty("id").GetGuid();
        var assigned = await Arrange.AssignCollarAsync(client, ranchId, deviceId, cattle.GetProperty("id").GetGuid(),
            collar.GetProperty("version").GetGuid());

        using var release = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices/{deviceId}/releases", new
        {
            reason = "Collar maintenance",
            expectedVersion = assigned.GetProperty("version").GetGuid()
        });

        var released = await release.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Available", released.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, released.GetProperty("currentAssignment").ValueKind);
        using var history = await client.GetAsync($"api/v1/ranches/{ranchId}/devices/{deviceId}/assignments");
        var assignment = Assert.Single((await history.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.NotEqual(JsonValueKind.Null, assignment.GetProperty("endedAt").ValueKind);
        Assert.Equal("Collar maintenance", assignment.GetProperty("releaseReason").GetString());
    }

    [Fact]
    public async Task AssignCollar_WithoutActiveSubscription_IsForbidden()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0102");
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, SerialNumber());

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices/{collar.GetProperty("id").GetGuid()}/assignments", new
        {
            targetId = cattle.GetProperty("id").GetGuid(),
            expectedVersion = collar.GetProperty("version").GetGuid()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("IoTAssets.SubscriptionRequired", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RegisterEdgeGateway_ReturnsKeyOnceAndHidesItAfterwards()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var registration = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");

        var gatewayId = registration.GetProperty("id").GetGuid();
        Assert.StartsWith($"{gatewayId:D}.", registration.GetProperty("key").GetString());
        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/edge-gateways/{gatewayId}");
        var gateway = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Active", gateway.GetProperty("status").GetString());
        Assert.False(gateway.TryGetProperty("key", out _));
    }

    [Fact]
    public async Task RevokeEdgeGateway_RejectsItsKeyOnMonitoringRules()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var registration = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");
        var gatewayId = registration.GetProperty("id").GetGuid();
        var key = registration.GetProperty("key").GetString()!;
        using (var beforeRevocation = await client.GetWithGatewayKeyAsync($"api/v1/edge-gateways/{gatewayId}/monitoring-rules", key))
        {
            // Without any rule revision the gateway receives 304, which proves that its key is accepted before revocation.
            Assert.Equal(HttpStatusCode.NotModified, beforeRevocation.StatusCode);
        }

        using var revocation = await client.PostAsync($"api/v1/ranches/{ranchId}/edge-gateways/{gatewayId}/revocations", content: null);

        var revoked = await revocation.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Revoked", revoked.GetProperty("status").GetString());
        Assert.NotEqual(JsonValueKind.Null, revoked.GetProperty("revokedAt").ValueKind);
        using var afterRevocation = await client.GetWithGatewayKeyAsync($"api/v1/edge-gateways/{gatewayId}/monitoring-rules", key);
        await afterRevocation.ReadBodyAsync(HttpStatusCode.Unauthorized);
    }

    private static string SerialNumber() => $"COL-{Guid.NewGuid():N}";
}
