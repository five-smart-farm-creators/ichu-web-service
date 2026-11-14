using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.OperationsMonitoring;

[Collection(ApiCollection.Name)]
public sealed class OperationsMonitoringApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task ConfigureThreshold_ReturnsRevisionedThresholdAndListsIt()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var threshold = await Arrange.ConfigureFeverThresholdAsync(client, ranchId);

        Assert.Equal("BodyTemperature", threshold.GetProperty("metric").GetString());
        Assert.Equal(37.5, threshold.GetProperty("minValue").GetDouble());
        Assert.Equal(39.5, threshold.GetProperty("maxValue").GetDouble());
        Assert.Equal("Warning", threshold.GetProperty("severity").GetString());
        Assert.Equal(1, threshold.GetProperty("revision").GetInt64());
        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds");
        var listed = Assert.Single((await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal("MilkingCow", listed.GetProperty("lifeStage").GetString());
    }

    [Fact]
    public async Task ConfigureThreshold_WithInvertedBounds_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds", new
        {
            lifeStage = "MilkingCow",
            metric = "BodyTemperature",
            minValue = 39.5,
            maxValue = 37.5,
            severity = "Warning"
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.ThresholdRangeInvalid", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ConfigureThreshold_AcceptsWarningAndCriticalLevelsOfTheSameMetric()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        using var warningResponse = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds", HeartRateThreshold(48, 84, "Warning"));
        await warningResponse.ReadJsonAsync(HttpStatusCode.OK);

        using var criticalResponse = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds", HeartRateThreshold(40, 90, "Critical"));

        var critical = await criticalResponse.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Critical", critical.GetProperty("severity").GetString());
        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds");
        Assert.Equal(2, (await response.ReadJsonAsync(HttpStatusCode.OK)).GetArrayLength());
    }

    [Fact]
    public async Task ConfigureThreshold_CriticalRangeThatDoesNotContainTheWarningRange_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        using var warningResponse = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds", HeartRateThreshold(48, 84, "Warning"));
        await warningResponse.ReadJsonAsync(HttpStatusCode.OK);

        using var response = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/monitoring-thresholds", HeartRateThreshold(50, 90, "Critical"));

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.ThresholdLevelsInconsistent", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DefineGeofence_ForLot_ReturnsCreatedBoundary()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lot = await Arrange.CreateLotAsync(client, ranchId, "North pasture");
        var lotId = lot.GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/geofences", GeofenceRequest(lotId, "North pasture boundary"));

        var geofence = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(ranchId, geofence.GetProperty("ranchId").GetGuid());
        Assert.Equal(lotId, geofence.GetProperty("lotId").GetGuid());
        Assert.Equal("North pasture boundary", geofence.GetProperty("name").GetString());
        Assert.Equal(10, geofence.GetProperty("toleranceMeters").GetInt32());
        Assert.Equal("Critical", geofence.GetProperty("severity").GetString());
        Assert.Equal(1, geofence.GetProperty("revision").GetInt64());
        Assert.Equal(4, geofence.GetProperty("vertices").GetArrayLength());
    }

    [Fact]
    public async Task DefineGeofence_SecondTimeForSameLot_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = (await Arrange.CreateLotAsync(client, ranchId, "North pasture")).GetProperty("id").GetGuid();
        await client.PostJsonAsync($"api/v1/ranches/{ranchId}/geofences", GeofenceRequest(lotId, "First boundary"));

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/geofences", GeofenceRequest(lotId, "Second boundary"));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("OperationsMonitoring.GeofenceAlreadyDefined", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DefineGeofence_WithTwoVertices_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = (await Arrange.CreateLotAsync(client, ranchId, "North pasture")).GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/geofences", new
        {
            lotId,
            name = "Degenerate boundary",
            toleranceMeters = 10,
            severity = "Critical",
            vertices = new[] { new { latitude = -13.5, longitude = -72.1 }, new { latitude = -13.4, longitude = -72.0 } }
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.GeofenceVertexCount", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RegisterFieldEvent_AndClinicalIntervention_AreListedForTheAnimal()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0300", lifeStage: 3)).GetProperty("id").GetGuid();
        var occurredAt = DateTimeOffset.UtcNow.AddHours(-1);

        using var fieldEvent = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/field-events", new
        {
            operationId = Guid.NewGuid(),
            cattleId,
            type = "HealthEvent",
            occurredAt,
            observation = "Mild lameness"
        });
        using var intervention = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/clinical-interventions", new
        {
            operationId = Guid.NewGuid(),
            cattleId,
            type = "Vaccination",
            occurredAt,
            product = "Foot-and-mouth vaccine",
            dose = "2 ml"
        });

        var recordedEvent = await fieldEvent.ReadJsonAsync(HttpStatusCode.Created);
        Assert.False(recordedEvent.GetProperty("isClinical").GetBoolean());
        var recordedIntervention = await intervention.ReadJsonAsync(HttpStatusCode.Created);
        Assert.True(recordedIntervention.GetProperty("isClinical").GetBoolean());
        Assert.Equal("Foot-and-mouth vaccine", recordedIntervention.GetProperty("product").GetString());
        using var history = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/cattle/{cattleId}/livestock-operations?from={(occurredAt.AddDays(-1)).ToQueryValue()}&to={DateTimeOffset.UtcNow.AddHours(1).ToQueryValue()}");
        var operations = await history.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(2, operations.GetArrayLength());
    }

    [Fact]
    public async Task RegisterFieldEvent_RetriedWithSameOperation_ReturnsOkWithoutDuplicating()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0301", lifeStage: 3)).GetProperty("id").GetGuid();
        var request = new
        {
            operationId = Guid.NewGuid(),
            cattleId,
            type = "Birth",
            occurredAt = DateTimeOffset.UtcNow.AddHours(-2)
        };
        using var first = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/field-events", request);
        await first.ReadBodyAsync(HttpStatusCode.Created);

        using var retried = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/field-events", request);

        await retried.ReadBodyAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegisterFieldEvent_WithClinicalType_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0302", lifeStage: 3)).GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/field-events", new
        {
            operationId = Guid.NewGuid(),
            cattleId,
            type = "Treatment",
            occurredAt = DateTimeOffset.UtcNow.AddHours(-1)
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.FieldEventTypeOnly", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MobileSync_AcceptsValidOperationsAndReportsRejectedDuplicateAndConflictingEntries()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-MOBILE-SYNC")).GetProperty("id").GetGuid();
        var operationId = Guid.NewGuid();
        var invalidId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow.AddHours(-1);
        var entry = new { operationId, cattleId, type = "HealthEvent", occurredAt, observation = "Mild lameness" };
        var batch = new { operations = new object[] { entry, new { operationId = invalidId, cattleId, occurredAt } } };

        using var response = await client.PostJsonAsync("api/v1/mobile-sync/batch", batch);

        var results = (await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray().ToList();
        Assert.Equal("Accepted", Assert.Single(results, result => result.GetProperty("id").GetGuid() == operationId).GetProperty("status").GetString());
        Assert.Equal("Rejected", Assert.Single(results, result => result.GetProperty("id").GetGuid() == invalidId).GetProperty("status").GetString());
        using var replay = await client.PostJsonAsync("api/v1/mobile-sync/batch", batch);
        var repeated = (await replay.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray().ToList();
        Assert.Equal("Duplicate", Assert.Single(repeated, result => result.GetProperty("id").GetGuid() == operationId).GetProperty("status").GetString());
        using var conflict = await client.PostJsonAsync("api/v1/mobile-sync/batch", new
        {
            operations = new[] { new { operationId, cattleId, type = "HealthEvent", occurredAt, observation = "Different content" } }
        });
        var conflicted = Assert.Single((await conflict.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray());
        Assert.Equal("Conflict", conflicted.GetProperty("status").GetString());
        using var history = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/livestock-operations" +
            $"?from={occurredAt.AddDays(-1).ToQueryValue()}&to={DateTimeOffset.UtcNow.AddHours(1).ToQueryValue()}");
        Assert.Single((await history.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
    }

    [Fact]
    public async Task MobileSync_AnOperationForAnotherRanch_IsRejectedWithoutBeingSaved()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(owner, ranchId, "PE-MOBILE-FORBIDDEN")).GetProperty("id").GetGuid();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PostJsonAsync("api/v1/mobile-sync/batch", new
        {
            operations = new[] { new { operationId = Guid.NewGuid(), cattleId, type = "HealthEvent", occurredAt = DateTimeOffset.UtcNow.AddHours(-1), observation = "Mild lameness" } }
        });

        var result = Assert.Single((await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("results").EnumerateArray());
        Assert.Equal("Rejected", result.GetProperty("status").GetString());
        Assert.False(string.IsNullOrWhiteSpace(result.GetProperty("reason").GetString()));
        using var history = await owner.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/livestock-operations" +
            $"?from={DateTimeOffset.UtcNow.AddDays(-1).ToQueryValue()}&to={DateTimeOffset.UtcNow.AddHours(1).ToQueryValue()}");
        Assert.Empty((await history.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
    }

    [Fact]
    public async Task MobileSync_RequiresAuthenticationAndANonEmptyBatch()
    {
        using var anonymous = factory.CreateClient();
        using var unauthorized = await anonymous.PostJsonAsync("api/v1/mobile-sync/batch", new { operations = Array.Empty<object>() });
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);
        using var manager = factory.CreateClient(TestIdentity.Create());
        await manager.RegisterManagerAsync();

        using var empty = await manager.PostJsonAsync("api/v1/mobile-sync/batch", new { operations = Array.Empty<object>() });

        var problem = await empty.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("OperationsMonitoring.InvalidMobileBatchSize", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task LatestTelemetry_ForAnimalWithoutReadings_ReportsUnavailable()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0303", lifeStage: 3)).GetProperty("id").GetGuid();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/telemetry/latest");

        var latest = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(cattleId, latest.GetProperty("cattleId").GetGuid());
        Assert.Equal("Unavailable", latest.GetProperty("bodyTemperature").GetProperty("status").GetString());
        Assert.Equal("Unavailable", latest.GetProperty("lastKnownPosition").GetProperty("status").GetString());
    }

    [Fact]
    public async Task TelemetryHistory_ForAnimalWithoutReadings_IsEmpty()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0304", lifeStage: 3)).GetProperty("id").GetGuid();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/cattle/{cattleId}/telemetry?from={DateTimeOffset.UtcNow.AddDays(-1).ToQueryValue()}&to={DateTimeOffset.UtcNow.ToQueryValue()}");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(0, history.GetProperty("totalCount").GetInt32());
        Assert.Empty(history.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task ListAlerts_ForRanchWithoutAlerts_IsEmptyPage()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/alerts");

        var page = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(0, page.GetProperty("totalCount").GetInt32());
        Assert.Empty(page.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task AcknowledgeAlert_WhenAlertDoesNotExist_ReturnsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PatchJsonAsync($"api/v1/ranches/{ranchId}/alerts/{Guid.NewGuid()}", new
        {
            status = "Acknowledged",
            finding = "Checked",
            operationId = Guid.NewGuid(),
            expectedVersion = 1
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.AlertNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SynchronizeBatch_WithoutGatewayKey_IsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.PostJsonAsync("api/v1/telemetry/batch", new { readings = Array.Empty<object>() });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("OperationsMonitoring.GatewayUnauthenticated", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MonitoringRules_WithoutGatewayKey_IsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/edge-gateways/{Guid.NewGuid()}/monitoring-rules");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("OperationsMonitoring.GatewayUnauthenticated", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MonitoringRules_ReturnsRevisionAfterChangesAndNotModifiedWhenCurrent()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        await Arrange.ConfigureFeverThresholdAsync(client, monitored.RanchId);
        var lotId = (await Arrange.CreateLotAsync(client, monitored.RanchId, "North pasture")).GetProperty("id").GetGuid();
        await client.PostJsonAsync($"api/v1/ranches/{monitored.RanchId}/geofences", GeofenceRequest(lotId, "North pasture boundary"));

        using var response = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{monitored.GatewayId}/monitoring-rules?afterRevision=0", monitored.GatewayKey);
        using var current = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{monitored.GatewayId}/monitoring-rules?afterRevision=2", monitored.GatewayKey);

        var rules = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(2, rules.GetProperty("revision").GetInt64());
        Assert.Single(rules.GetProperty("thresholds").EnumerateArray());
        Assert.Single(rules.GetProperty("geofences").EnumerateArray());
        await current.ReadBodyAsync(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task MonitoringRules_ForAnotherGateway_IsForbidden()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var otherGateway = await Arrange.RegisterGatewayAsync(client, monitored.RanchId, "Second gateway");

        using var response = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{otherGateway.GetProperty("id").GetGuid()}/monitoring-rules", monitored.GatewayKey);

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("OperationsMonitoring.GatewayMismatch", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SynchronizeBatch_AcceptsReadingAndAlertOfAssignedCollar()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        await Arrange.ConfigureFeverThresholdAsync(client, monitored.RanchId);
        var readingId = Guid.NewGuid();
        var alertId = Guid.NewGuid();

        var results = await Arrange.SynchronizeFeverAsync(client, monitored.GatewayKey, monitored.DeviceId, readingId, alertId,
            40.2, DateTimeOffset.UtcNow);

        var entries = results.GetProperty("results").EnumerateArray().ToList();
        Assert.Equal(2, entries.Count);
        Assert.Equal("Reading", entries[0].GetProperty("kind").GetString());
        Assert.Equal("Accepted", entries[0].GetProperty("status").GetString());
        Assert.Equal("Alert", entries[1].GetProperty("kind").GetString());
        Assert.Equal("Accepted", entries[1].GetProperty("status").GetString());
        using var alerts = await client.GetAsync($"api/v1/ranches/{monitored.RanchId}/alerts");
        var alert = Assert.Single((await alerts.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("items").EnumerateArray());
        Assert.Equal(alertId, alert.GetProperty("alertId").GetGuid());
        Assert.Equal(monitored.CattleId, alert.GetProperty("cattleId").GetGuid());
        Assert.Equal("Pending", alert.GetProperty("status").GetString());
        Assert.Equal(40.2, alert.GetProperty("detectedValue").GetDouble());
        Assert.Equal(39.5, alert.GetProperty("thresholdMaxValue").GetDouble());
        Assert.Contains(readingId, alert.GetProperty("readingIds").EnumerateArray().Select(id => id.GetGuid()));
    }

    [Fact]
    public async Task SynchronizeBatch_RepeatedBatch_ReportsEntriesAsDuplicates()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        await Arrange.ConfigureFeverThresholdAsync(client, monitored.RanchId);
        var readingId = Guid.NewGuid();
        var alertId = Guid.NewGuid();
        var recordedAt = DateTimeOffset.UtcNow;
        await Arrange.SynchronizeFeverAsync(client, monitored.GatewayKey, monitored.DeviceId, readingId, alertId, 40.2, recordedAt);

        var replay = await Arrange.SynchronizeFeverAsync(client, monitored.GatewayKey, monitored.DeviceId, readingId, alertId, 40.2, recordedAt);

        Assert.All(replay.GetProperty("results").EnumerateArray(),
            entry => Assert.Equal("Duplicate", entry.GetProperty("status").GetString()));
    }

    [Fact]
    public async Task SynchronizeBatch_WithReadingOfUnknownDevice_RejectsIt()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);

        var results = await Arrange.SynchronizeReadingAsync(client, monitored.GatewayKey, Guid.NewGuid(), Guid.NewGuid(), 38.6,
            DateTimeOffset.UtcNow);

        var entry = Assert.Single(results.GetProperty("results").EnumerateArray());
        Assert.Equal("Rejected", entry.GetProperty("status").GetString());
        Assert.Equal("Reading", entry.GetProperty("kind").GetString());
    }

    [Fact]
    public async Task AcknowledgeAlert_RecordsAttentionOnce_AndRejectsStaleVersion()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        await Arrange.ConfigureFeverThresholdAsync(client, monitored.RanchId);
        var alertId = Guid.NewGuid();
        await Arrange.SynchronizeFeverAsync(client, monitored.GatewayKey, monitored.DeviceId, Guid.NewGuid(), alertId, 40.2,
            DateTimeOffset.UtcNow);
        var operationId = Guid.NewGuid();
        var acknowledgement = new
        {
            status = "Acknowledged",
            finding = "Fever confirmed by the veterinarian",
            action = "Isolated from the herd",
            operationId,
            expectedVersion = 1
        };

        using var first = await client.PatchJsonAsync($"api/v1/ranches/{monitored.RanchId}/alerts/{alertId}", acknowledgement);
        var acknowledged = await first.ReadJsonAsync(HttpStatusCode.OK);
        using var retried = await client.PatchJsonAsync($"api/v1/ranches/{monitored.RanchId}/alerts/{alertId}", acknowledgement);
        await retried.ReadBodyAsync(HttpStatusCode.OK);
        using var stale = await client.PatchJsonAsync($"api/v1/ranches/{monitored.RanchId}/alerts/{alertId}", new
        {
            status = "Acknowledged",
            finding = "A different finding",
            operationId = Guid.NewGuid(),
            expectedVersion = 1
        });

        Assert.Equal("Acknowledged", acknowledged.GetProperty("status").GetString());
        Assert.Equal(2, acknowledged.GetProperty("version").GetInt32());
        Assert.Equal("Fever confirmed by the veterinarian", acknowledged.GetProperty("finding").GetString());
        Assert.Equal(await client.GetCurrentAccountIdAsync(), acknowledged.GetProperty("acknowledgedBy").GetGuid());
        var conflict = await stale.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal(alertId, conflict.GetProperty("alertId").GetGuid());
        Assert.Equal(2, conflict.GetProperty("version").GetInt32());
    }

    [Fact]
    public async Task LatestTelemetry_ReturnsLatestValidTemperature()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var readingId = Guid.NewGuid();
        await Arrange.SynchronizeReadingAsync(client, monitored.GatewayKey, monitored.DeviceId, readingId, 38.6, DateTimeOffset.UtcNow);

        using var response = await client.GetAsync($"api/v1/ranches/{monitored.RanchId}/cattle/{monitored.CattleId}/telemetry/latest");

        var latest = await response.ReadJsonAsync(HttpStatusCode.OK);
        var temperature = latest.GetProperty("bodyTemperature");
        Assert.Equal("Available", temperature.GetProperty("status").GetString());
        Assert.Equal(38.6, temperature.GetProperty("value").GetDouble());
        Assert.Equal("celsius", temperature.GetProperty("unit").GetString());
        Assert.Equal("Unavailable", latest.GetProperty("activityIndex").GetProperty("status").GetString());
        Assert.Equal("Unavailable", latest.GetProperty("heartRate").GetProperty("status").GetString());
        Assert.Equal("Unavailable", latest.GetProperty("methaneIndex").GetProperty("status").GetString());
    }

    [Fact]
    public async Task TelemetryHistory_ListsSynchronizedReadingsOfThePeriod()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var readingId = Guid.NewGuid();
        var recordedAt = DateTimeOffset.UtcNow;
        await Arrange.SynchronizeReadingAsync(client, monitored.GatewayKey, monitored.DeviceId, readingId, 38.6, recordedAt);

        using var response = await client.GetAsync(
            $"api/v1/ranches/{monitored.RanchId}/cattle/{monitored.CattleId}/telemetry?from={recordedAt.AddHours(-1).ToQueryValue()}&to={recordedAt.AddHours(1).ToQueryValue()}");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(1, history.GetProperty("totalCount").GetInt32());
        var reading = Assert.Single(history.GetProperty("items").EnumerateArray());
        Assert.Equal(readingId, reading.GetProperty("readingId").GetGuid());
        Assert.Equal(38.6, reading.GetProperty("bodyTemperature").GetProperty("value").GetDouble());
        Assert.Equal("Valid", reading.GetProperty("bodyTemperature").GetProperty("status").GetString());
    }

    [Fact]
    public async Task TelemetryHistory_KeepsHeartRateAndMethaneIndexOfEachReading()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var readingId = Guid.NewGuid();
        var recordedAt = DateTimeOffset.UtcNow;
        using var synchronization = await client.PostJsonWithGatewayKeyAsync("api/v1/telemetry/batch", monitored.GatewayKey, new
        {
            readings = new[]
            {
                new
                {
                    readingId,
                    deviceId = monitored.DeviceId,
                    recordedAt,
                    heartRate = new { value = 72.0 },
                    methaneIndex = new { invalidReason = "Gas sensor warming up" }
                }
            }
        });
        await synchronization.ReadJsonAsync(HttpStatusCode.OK);

        using var response = await client.GetAsync(
            $"api/v1/ranches/{monitored.RanchId}/cattle/{monitored.CattleId}/telemetry?from={recordedAt.AddHours(-1).ToQueryValue()}&to={recordedAt.AddHours(1).ToQueryValue()}");

        var history = await response.ReadJsonAsync(HttpStatusCode.OK);
        var reading = Assert.Single(history.GetProperty("items").EnumerateArray());
        Assert.Equal(72.0, reading.GetProperty("heartRate").GetProperty("value").GetDouble());
        Assert.Equal("bpm", reading.GetProperty("heartRate").GetProperty("unit").GetString());
        Assert.Equal("Invalid", reading.GetProperty("methaneIndex").GetProperty("status").GetString());
        Assert.Equal("Gas sensor warming up", reading.GetProperty("methaneIndex").GetProperty("invalidReason").GetString());
    }

    [Fact]
    public async Task AcknowledgeAlert_OfAnotherRanch_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(owner);
        await Arrange.ConfigureFeverThresholdAsync(owner, monitored.RanchId);
        var alertId = Guid.NewGuid();
        await Arrange.SynchronizeFeverAsync(owner, monitored.GatewayKey, monitored.DeviceId, Guid.NewGuid(), alertId, 40.2,
            DateTimeOffset.UtcNow);
        using var other = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await other.RegisterManagerAsync();

        using var response = await other.PatchJsonAsync($"api/v1/ranches/{otherRanchId}/alerts/{alertId}", new
        {
            status = "Acknowledged",
            finding = "Checked",
            operationId = Guid.NewGuid(),
            expectedVersion = 1
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.AlertNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task LatestTelemetry_OfCattleOfAnotherRanch_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(owner);
        using var other = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await other.RegisterManagerAsync();

        using var response = await other.GetAsync($"api/v1/ranches/{otherRanchId}/cattle/{monitored.CattleId}/telemetry/latest");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("OperationsMonitoring.AnimalNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeviceAssignments_WithoutGatewayKey_IsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/edge-gateways/{Guid.NewGuid()}/device-assignments");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("OperationsMonitoring.GatewayUnauthenticated", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeviceAssignments_ForAnotherGateway_IsForbidden()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var otherGateway = await Arrange.RegisterGatewayAsync(client, monitored.RanchId, "Second gateway");

        using var response = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{otherGateway.GetProperty("id").GetGuid()}/device-assignments", monitored.GatewayKey);

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("OperationsMonitoring.GatewayMismatch", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DeviceAssignments_ListsEachCollarWithTheLotAndStageOfItsAnimal()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        var lot = await Arrange.CreateLotAsync(client, monitored.RanchId, "Milking pen");
        var lotId = lot.GetProperty("id").GetGuid();
        await Arrange.AssignToLotAsync(client, monitored.RanchId, lotId, monitored.CattleId, lot.GetProperty("version").GetGuid());
        var heifer = await Arrange.RegisterCattleAsync(client, monitored.RanchId, "PE-0201", lifeStage: 2);
        var heiferId = heifer.GetProperty("id").GetGuid();
        var heiferCollar = await Arrange.RegisterCollarAsync(client, monitored.RanchId, $"COL-{Guid.NewGuid():N}");
        var heiferCollarId = heiferCollar.GetProperty("id").GetGuid();
        await Arrange.AssignCollarAsync(client, monitored.RanchId, heiferCollarId, heiferId, heiferCollar.GetProperty("version").GetGuid());

        using var response = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{monitored.GatewayId}/device-assignments", monitored.GatewayKey);

        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(monitored.RanchId, body.GetProperty("ranchId").GetGuid());
        var assignments = body.GetProperty("assignments").EnumerateArray().ToList();
        var deviceIds = assignments.Select(assignment => assignment.GetProperty("deviceId").GetGuid()).ToList();
        Assert.Equal(deviceIds.OrderBy(id => id), deviceIds);
        var byDevice = assignments.ToDictionary(assignment => assignment.GetProperty("deviceId").GetGuid());
        var milking = byDevice[monitored.DeviceId];
        Assert.Equal(monitored.CattleId, milking.GetProperty("cattleId").GetGuid());
        Assert.Equal(lotId, milking.GetProperty("lotId").GetGuid());
        Assert.Equal("MilkingCow", milking.GetProperty("lifeStage").GetString());
        var heiferAssignment = byDevice[heiferCollarId];
        Assert.Equal(heiferId, heiferAssignment.GetProperty("cattleId").GetGuid());
        Assert.Equal(JsonValueKind.Null, heiferAssignment.GetProperty("lotId").ValueKind);
        Assert.Equal("Heifer", heiferAssignment.GetProperty("lifeStage").GetString());
    }

    [Fact]
    public async Task DeviceAssignments_OmitsTheCollarOfAnExitedAnimal()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var monitored = await MonitorCattleAsync(client);
        using var cattleResponse = await client.GetAsync($"api/v1/ranches/{monitored.RanchId}/cattle/{monitored.CattleId}");
        var cattleVersion = (await cattleResponse.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("cattle").GetProperty("version").GetGuid();
        using var deactivation = await client.PostJsonAsync($"api/v1/ranches/{monitored.RanchId}/cattle/{monitored.CattleId}/deactivations", new
        {
            reason = 2,
            exitDate = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = cattleVersion
        });
        await deactivation.ReadBodyAsync(HttpStatusCode.OK);

        using var response = await client.GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{monitored.GatewayId}/device-assignments", monitored.GatewayKey);

        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Empty(body.GetProperty("assignments").EnumerateArray());
    }

    [Theory]
    [InlineData("monitoring-rules")]
    [InlineData("device-assignments")]
    public async Task OpenApiDocument_DeclaresTheGatewayKeyOnEachEdgeGatewayDownload(string download)
    {
        using var host = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["OpenApi:Enabled"] = "true" })));
        using var client = host.CreateClient();

        using var response = await client.GetAsync("openapi/v1.json");

        var document = await response.ReadJsonAsync(HttpStatusCode.OK);
        var operation = document.GetProperty("paths").GetProperty($"/api/v1/edge-gateways/{{gatewayId}}/{download}").GetProperty("get");
        var header = Assert.Single(operation.GetProperty("parameters").EnumerateArray(),
            parameter => parameter.GetProperty("name").GetString() == "X-Gateway-Key");
        Assert.Equal("header", header.GetProperty("in").GetString());
        Assert.True(header.GetProperty("required").GetBoolean());
    }

    /// <summary>
    ///     Builds a ranch whose collar is assigned to an animal and whose edge gateway holds a valid key.
    /// </summary>
    private async Task<MonitoredRanch> MonitorCattleAsync(HttpClient client)
    {
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0200", lifeStage: 3)).GetProperty("id").GetGuid();
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, $"COL-{Guid.NewGuid():N}");
        var deviceId = collar.GetProperty("id").GetGuid();
        await Arrange.AssignCollarAsync(client, ranchId, deviceId, cattleId, collar.GetProperty("version").GetGuid());
        var gateway = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");
        return new MonitoredRanch(ranchId, cattleId, deviceId, gateway.GetProperty("id").GetGuid(), gateway.GetProperty("key").GetString()!);
    }

    private static object HeartRateThreshold(double minValue, double maxValue, string severity) => new
    {
        lifeStage = "MilkingCow",
        metric = "HeartRate",
        minValue,
        maxValue,
        severity
    };

    private static object GeofenceRequest(Guid lotId, string name) => new
    {
        lotId,
        name,
        toleranceMeters = 10,
        severity = "Critical",
        vertices = new[]
        {
            new { latitude = -13.5, longitude = -72.1 },
            new { latitude = -13.5, longitude = -72.09 },
            new { latitude = -13.49, longitude = -72.09 },
            new { latitude = -13.49, longitude = -72.1 }
        }
    };

    private sealed record MonitoredRanch(Guid RanchId, Guid CattleId, Guid DeviceId, Guid GatewayId, string GatewayKey);
}
