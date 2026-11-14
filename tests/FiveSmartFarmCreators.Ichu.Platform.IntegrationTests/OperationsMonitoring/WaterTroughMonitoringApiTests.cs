using System.Net;
using System.Globalization;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.OperationsMonitoring;

[Collection(ApiCollection.Name)]
public sealed class WaterTroughMonitoringApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task ReceiveReading_IsIdempotentAndControllerHistoryIsPaged()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var setup = await CreateAssignedControllerAsync(client, ranchId, "Main trough");
        var request = ReadingRequest(Guid.NewGuid(), AlignedUtcNow().AddSeconds(1), temperature: 16.5m);

        using var firstResponse = await client.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey, request);
        var first = await firstResponse.ReadJsonAsync(HttpStatusCode.Created);
        using var duplicateResponse = await client.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey, request);
        var duplicate = await duplicateResponse.ReadJsonAsync(HttpStatusCode.OK);
        using var conflictResponse = await client.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey,
            request with { Temperature = 16.6m });
        var conflict = await conflictResponse.ReadJsonAsync(HttpStatusCode.Conflict);

        Assert.Equal(first.GetProperty("id").GetGuid(), duplicate.GetProperty("id").GetGuid());
        Assert.Equal(first.GetProperty("receivedAt").GetDateTimeOffset(), duplicate.GetProperty("receivedAt").GetDateTimeOffset());
        Assert.Equal(request.ReadingId, first.GetProperty("readingId").GetGuid());
        Assert.Equal(setup.ControllerId, first.GetProperty("controllerId").GetGuid());
        Assert.Equal(setup.WaterTroughId, first.GetProperty("waterTroughId").GetGuid());
        Assert.Equal(ranchId, first.GetProperty("ranchId").GetGuid());
        Assert.Equal("Celsius", first.GetProperty("temperatureUnit").GetString());
        Assert.Equal("On", first.GetProperty("heaterState").GetString());
        Assert.Equal("EdgeGateway", first.GetProperty("origin").GetString());
        Assert.Equal("OperationsMonitoring.WaterReadingIdentifierConflict", conflict.GetProperty("code").GetString());

        var from = request.CapturedAt.AddHours(-1);
        var to = request.CapturedAt.AddHours(1);
        using var historyResponse = await client.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={from.ToQueryValue()}&to={to.ToQueryValue()}&page=1&pageSize=1");
        var history = await historyResponse.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(1, history.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, history.GetProperty("items").GetArrayLength());
        Assert.Equal(request.ReadingId,
            history.GetProperty("items")[0].GetProperty("readingId").GetGuid());

        using var invalidPage = await client.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={from.ToQueryValue()}&to={to.ToQueryValue()}&pageSize=501");
        var invalidPageProblem = await invalidPage.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.InvalidPage", invalidPageProblem.GetProperty("code").GetString());

        var subMicrosecondFrom = from.AddTicks(1);
        using var invalidPrecision = await client.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={subMicrosecondFrom.ToQueryValue()}&to={to.ToQueryValue()}");
        var invalidPrecisionProblem = await invalidPrecision.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.WaterReadingPeriodPrecisionInvalid",
            invalidPrecisionProblem.GetProperty("code").GetString());

        var localFrom = Uri.EscapeDataString(from.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
        var localTo = Uri.EscapeDataString(to.ToString("yyyy-MM-dd'T'HH:mm:ss.ffffff", CultureInfo.InvariantCulture));
        using var missingOffset = await client.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={localFrom}&to={localTo}");
        var missingOffsetProblem = await missingOffset.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.WaterReadingPeriodOffsetRequired",
            missingOffsetProblem.GetProperty("code").GetString());

        var subMicrosecondRequest = request with { ReadingId = Guid.NewGuid(), CapturedAt = request.CapturedAt.AddTicks(1) };
        using var invalidCapture = await client.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey, subMicrosecondRequest);
        var invalidCaptureProblem = await invalidCapture.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.WaterReadingInvalid", invalidCaptureProblem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ReceiveReading_ConcurrentIdenticalRequestsCreateOneRecord()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var setup = await CreateAssignedControllerAsync(client, ranchId, "Concurrent trough");
        var request = ReadingRequest(Guid.NewGuid(), AlignedUtcNow().AddSeconds(1), 18m);
        var path = $"api/v1/water-controllers/{setup.ControllerId}/readings";

        var responses = await Task.WhenAll(
            client.PostJsonWithGatewayKeyAsync(path, setup.GatewayKey, request),
            client.PostJsonWithGatewayKeyAsync(path, setup.GatewayKey, request));
        try
        {
            Assert.Equal(new[] { HttpStatusCode.OK, HttpStatusCode.Created },
                responses.Select(response => response.StatusCode).OrderBy(status => (int)status));
            var body = await responses[0].ReadJsonAsync(responses[0].StatusCode);
            Assert.Equal(request.ReadingId, body.GetProperty("readingId").GetGuid());
        }
        finally
        {
            foreach (var response in responses)
            {
                response.Dispose();
            }
        }

        var from = request.CapturedAt.AddHours(-1);
        var to = request.CapturedAt.AddHours(1);
        using var history = await client.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={from.ToQueryValue()}&to={to.ToQueryValue()}");
        var result = await history.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(1, result.GetProperty("totalCount").GetInt32());
    }

    [Fact]
    public async Task ReceiveReading_EnforcesGatewayAndControllerAuthorization()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, owner, ranchId, Arrange.StandardPlanId);
        var setup = await CreateAssignedControllerAsync(owner, ranchId, "Authorized trough");
        var request = ReadingRequest(Guid.NewGuid(), AlignedUtcNow().AddSeconds(1), 14m);

        using var missingKey = await owner.PostJsonAsync($"api/v1/water-controllers/{setup.ControllerId}/readings", request);
        Assert.Equal(HttpStatusCode.Unauthorized, missingKey.StatusCode);

        using var invalidKey = await owner.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", "invalid.key", request);
        Assert.Equal(HttpStatusCode.Unauthorized, invalidKey.StatusCode);

        using var unknownController = await owner.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{Guid.NewGuid()}/readings", setup.GatewayKey, request);
        var unknownControllerProblem = await unknownController.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.WaterControllerNotFound", unknownControllerProblem.GetProperty("code").GetString());

        var unassignedController = await RegisterControllerAsync(owner, ranchId);
        using var unassigned = await owner.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{unassignedController.ControllerId}/readings", setup.GatewayKey, request);
        var unassignedProblem = await unassigned.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("OperationsMonitoring.WaterControllerGatewayAccessDenied", unassignedProblem.GetProperty("code").GetString());

        using var outsider = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await outsider.RegisterManagerAsync();
        var otherGateway = await Arrange.RegisterGatewayAsync(outsider, otherRanchId, "Other ranch gateway");
        var otherGatewayKey = otherGateway.GetProperty("key").GetString()!;
        using var wrongRanch = await outsider.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", otherGatewayKey, request);
        Assert.Equal(HttpStatusCode.Forbidden, wrongRanch.StatusCode);
    }

    [Fact]
    public async Task ReadingHistory_HidesOtherRanchesAndRequiresAUserToken()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, owner, ranchId, Arrange.StandardPlanId);
        var setup = await CreateAssignedControllerAsync(owner, ranchId, "Private trough");
        var reading = ReadingRequest(Guid.NewGuid(), AlignedUtcNow().AddSeconds(1), 12m);
        using var received = await owner.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey, reading);
        await received.ReadBodyAsync(HttpStatusCode.Created);

        using var anonymous = factory.CreateClient();
        var from = reading.CapturedAt.AddHours(-1);
        var to = reading.CapturedAt.AddHours(1);
        using var unauthenticated = await anonymous.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={from.ToQueryValue()}&to={to.ToQueryValue()}");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);

        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();
        using var crossRanch = await outsider.GetAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings?from={from.ToQueryValue()}&to={to.ToQueryValue()}");
        var problem = await crossRanch.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.WaterControllerNotFound", problem.GetProperty("code").GetString());

        using var crossRanchStatus = await outsider.GetAsync(
            $"api/v1/ranches/{ranchId}/water-troughs/{setup.WaterTroughId}/status");
        var statusProblem = await crossRanchStatus.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("OperationsMonitoring.Forbidden", statusProblem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task WaterTroughStatus_ListsNoControllerAndInvalidLatestReadingWithoutInventingTemperature()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var noController = await RegisterTroughAsync(client, ranchId, "A trough without a controller");
        var setup = await CreateAssignedControllerAsync(client, ranchId, "B trough with a controller");
        var invalidReading = ReadingRequest(Guid.NewGuid(), AlignedUtcNow().AddSeconds(1), 3.25m, isValid: false);
        using var receive = await client.PostJsonWithGatewayKeyAsync(
            $"api/v1/water-controllers/{setup.ControllerId}/readings", setup.GatewayKey, invalidReading);
        await receive.ReadBodyAsync(HttpStatusCode.Created);

        using var listResponse = await client.GetAsync($"api/v1/ranches/{ranchId}/water-troughs/status");
        var list = await listResponse.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(2, list.GetProperty("items").GetArrayLength());
        var noControllerStatus = list.GetProperty("items")[0];
        Assert.Equal(noController.GetProperty("id").GetGuid(), noControllerStatus.GetProperty("waterTroughId").GetGuid());
        Assert.Equal("NoController", noControllerStatus.GetProperty("controllerStatus").GetString());
        Assert.Equal("Unavailable", noControllerStatus.GetProperty("measurementStatus").GetString());
        Assert.Equal(JsonValueKind.Null, noControllerStatus.GetProperty("latestReading").ValueKind);

        var invalidStatus = list.GetProperty("items")[1];
        Assert.Equal(setup.WaterTroughId, invalidStatus.GetProperty("waterTroughId").GetGuid());
        Assert.Equal("Communicating", invalidStatus.GetProperty("controllerStatus").GetString());
        Assert.Equal("Invalid", invalidStatus.GetProperty("measurementStatus").GetString());
        Assert.Equal("Unavailable", invalidStatus.GetProperty("rangeStatus").GetString());
        Assert.Equal(3.25m, invalidStatus.GetProperty("latestReading").GetProperty("temperature").GetDecimal());

        using var itemResponse = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/water-troughs/{setup.WaterTroughId}/status");
        var item = await itemResponse.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(invalidStatus.GetProperty("waterTroughId").GetGuid(), item.GetProperty("waterTroughId").GetGuid());
        Assert.Equal(invalidReading.ReadingId, item.GetProperty("latestReading").GetProperty("readingId").GetGuid());
    }

    private static async Task<WaterControllerSetup> CreateAssignedControllerAsync(HttpClient client, Guid ranchId,
        string troughName)
    {
        var trough = await RegisterTroughAsync(client, ranchId, troughName);
        using var rangeResponse = await client.PutJsonAsync(
            $"api/v1/ranches/{ranchId}/water-troughs/{trough.GetProperty("id").GetGuid()}/temperature-range",
            new { minimumCelsius = 10m, maximumCelsius = 20m, expectedVersion = trough.GetProperty("version").GetGuid() });
        var configured = await rangeResponse.ReadJsonAsync(HttpStatusCode.OK);
        var controller = await RegisterControllerAsync(client, ranchId);
        using var assignmentResponse = await client.PostJsonAsync(
            $"api/v1/ranches/{ranchId}/devices/{controller.ControllerId}/assignments",
            new { targetId = trough.GetProperty("id").GetGuid(), expectedVersion = controller.Version });
        await assignmentResponse.ReadBodyAsync(HttpStatusCode.Created);
        var gateway = await Arrange.RegisterGatewayAsync(client, ranchId, $"Gateway {Guid.NewGuid():N}");
        return new WaterControllerSetup(ranchId, configured.GetProperty("id").GetGuid(), controller.ControllerId,
            gateway.GetProperty("key").GetString()!);
    }

    private static async Task<JsonElement> RegisterTroughAsync(HttpClient client, Guid ranchId, string name)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/water-troughs", new { name });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    private static async Task<RegisteredController> RegisterControllerAsync(HttpClient client, Guid ranchId)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/devices", new
        {
            serial = $"WC-{Guid.NewGuid():N}",
            type = 2,
            firmwareVersion = "1.0.0"
        });
        var controller = await response.ReadJsonAsync(HttpStatusCode.Created);
        return new RegisteredController(controller.GetProperty("id").GetGuid(), controller.GetProperty("version").GetGuid());
    }

    private static WaterControllerReadingResource ReadingRequest(Guid readingId, DateTimeOffset capturedAt,
        decimal temperature, bool isValid = true) =>
        new(readingId, temperature, "Celsius", isValid, "On", capturedAt);

    private static DateTimeOffset AlignedUtcNow()
    {
        var ticks = DateTimeOffset.UtcNow.UtcTicks;
        return new DateTimeOffset(ticks - ticks % 10, TimeSpan.Zero);
    }

    private sealed record WaterControllerSetup(Guid RanchId, Guid WaterTroughId, Guid ControllerId, string GatewayKey);
    private sealed record RegisteredController(Guid ControllerId, Guid Version);
    private sealed record WaterControllerReadingResource(Guid ReadingId, decimal Temperature, string TemperatureUnit,
        bool IsValid, string HeaterState, DateTimeOffset CapturedAt);
}
