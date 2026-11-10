using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.DashboardAnalytics;

[Collection(ApiCollection.Name)]
public sealed class DashboardAnalyticsApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task Dashboard_ForCurrentMonth_ReportsHerdWithoutTelemetry()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.RegisterCattleAsync(client, ranchId, "PE-0500", lifeStage: 3);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/dashboard?from={from.ToQueryValue()}&to={to.ToQueryValue()}");

        var dashboard = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(ranchId, dashboard.GetProperty("ranchId").GetGuid());
        Assert.Equal("ichu-platform", dashboard.GetProperty("source").GetString());
        var monitored = dashboard.GetProperty("monitoredAnimals");
        Assert.Equal(0, monitored.GetProperty("value").GetDouble());
        Assert.Equal(1, monitored.GetProperty("denominator").GetInt32());
        Assert.Equal(0, dashboard.GetProperty("mortalityRate").GetProperty("value").GetDouble());
        Assert.Equal(JsonValueKind.Null, dashboard.GetProperty("confirmedPregnancies").GetProperty("value").ValueKind);
        Assert.Equal(1, dashboard.GetProperty("excludedAnimals").GetInt32());
        Assert.Empty(dashboard.GetProperty("openAlertsByType").EnumerateObject());
    }

    [Fact]
    public async Task Dashboard_ForCurrentMonth_ReflectsTelemetryAndOpenAlerts()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0501", lifeStage: 3)).GetProperty("id").GetGuid();
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, $"COL-{Guid.NewGuid():N}");
        var deviceId = collar.GetProperty("id").GetGuid();
        await Arrange.AssignCollarAsync(client, ranchId, deviceId, cattleId, collar.GetProperty("version").GetGuid());
        var gateway = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");
        var gatewayKey = gateway.GetProperty("key").GetString()!;
        await Arrange.ConfigureFeverThresholdAsync(client, ranchId);
        await Arrange.SynchronizeFeverAsync(client, gatewayKey, deviceId, Guid.NewGuid(), Guid.NewGuid(), 40.2, DateTimeOffset.UtcNow);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/dashboard?from={from.ToQueryValue()}&to={to.ToQueryValue()}");

        var dashboard = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(1, dashboard.GetProperty("monitoredAnimals").GetProperty("value").GetDouble());
        Assert.Equal(1, dashboard.GetProperty("openAlertsByType").GetProperty("temperature").GetInt32());
    }

    [Fact]
    public async Task Trends_ForLotWithoutReadings_ReturnsEmptySeriesWithReason()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0502", lifeStage: 3)).GetProperty("id").GetGuid();
        var lot = await Arrange.CreateLotAsync(client, ranchId, "Milking group");
        await Arrange.AssignToLotAsync(client, ranchId, lot.GetProperty("id").GetGuid(), cattleId, lot.GetProperty("version").GetGuid());
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/trends?lotId={lot.GetProperty("id").GetGuid()}&from={from.ToQueryValue()}&to={to.ToQueryValue()}" +
            "&metric=body-temperature&deviation=2");

        var trend = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("body-temperature", trend.GetProperty("metric").GetString());
        Assert.Equal("celsius", trend.GetProperty("unit").GetString());
        Assert.Empty(trend.GetProperty("points").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, trend.GetProperty("lotAverage").ValueKind);
        Assert.Equal("There are no valid readings for this lot and metric.", trend.GetProperty("unavailableReason").GetString());
    }

    [Fact]
    public async Task Trends_WithUnsupportedMetric_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/trends?lotId={Guid.NewGuid()}&from={from.ToQueryValue()}&to={to.ToQueryValue()}" +
            "&metric=heart-rate&deviation=2");

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("DashboardAnalytics.UnsupportedMetric", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Trends_ForBodyTemperature_ReturnsReadingsOfTheLot()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0503", lifeStage: 3)).GetProperty("id").GetGuid();
        var lot = await Arrange.CreateLotAsync(client, ranchId, "Milking group");
        var lotId = lot.GetProperty("id").GetGuid();
        await Arrange.AssignToLotAsync(client, ranchId, lotId, cattleId, lot.GetProperty("version").GetGuid());
        var collar = await Arrange.RegisterCollarAsync(client, ranchId, $"COL-{Guid.NewGuid():N}");
        var deviceId = collar.GetProperty("id").GetGuid();
        await Arrange.AssignCollarAsync(client, ranchId, deviceId, cattleId, collar.GetProperty("version").GetGuid());
        var gateway = await Arrange.RegisterGatewayAsync(client, ranchId, "Corral gateway");
        await Arrange.SynchronizeReadingAsync(client, gateway.GetProperty("key").GetString()!, deviceId, Guid.NewGuid(), 38.6,
            DateTimeOffset.UtcNow);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/trends?lotId={lotId}&from={from.ToQueryValue()}&to={to.ToQueryValue()}" +
            "&metric=body-temperature&deviation=2");

        var trend = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(38.6, trend.GetProperty("lotAverage").GetDouble());
        var point = Assert.Single(trend.GetProperty("points").EnumerateArray());
        Assert.Equal(cattleId, point.GetProperty("cattleId").GetGuid());
        Assert.Equal(38.6, point.GetProperty("value").GetDouble());
        Assert.Empty(trend.GetProperty("outlierAnimals").EnumerateArray());
    }

    [Fact]
    public async Task ExecutiveReport_WithoutExportFeature_IsForbidden()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.StandardPlanId);
        await Arrange.RegisterCattleAsync(client, ranchId, "PE-0504", lifeStage: 3);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/reports/executive?from={from.ToQueryValue()}&to={to.ToQueryValue()}&format=json");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("DashboardAnalytics.ExportNotIncluded", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ExecutiveReport_ForRanchWithoutData_IsUnprocessable()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.ProPlanId);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/reports/executive?from={from.ToQueryValue()}&to={to.ToQueryValue()}&format=json");

        var problem = await response.ReadJsonAsync(HttpStatusCode.UnprocessableEntity);
        Assert.Equal("DashboardAnalytics.NoDataToExport", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ExecutiveReport_WithExportFeature_ReturnsJsonIndicators()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.ActivateSubscriptionAsync(factory, client, ranchId, Arrange.ProPlanId);
        await Arrange.RegisterCattleAsync(client, ranchId, "PE-0505", lifeStage: 3);
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/reports/executive?from={from.ToQueryValue()}&to={to.ToQueryValue()}&format=json");

        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var report = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.NotEqual(JsonValueKind.Null, report.GetProperty("generatedAt").ValueKind);
        Assert.Equal(1, report.GetProperty("indicators").GetProperty("mortalityRate").GetProperty("denominator").GetInt32());
    }

    [Fact]
    public async Task Portfolio_ListsRanchesOfTheAccount()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync("api/v1/analytics/portfolio");

        var entry = Assert.Single((await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(ranchId, entry.GetProperty("ranchId").GetGuid());
        Assert.Equal("ichu-platform", entry.GetProperty("source").GetString());
        Assert.Equal(0, entry.GetProperty("criticalAlerts").GetInt32());
        Assert.Equal(JsonValueKind.Null, entry.GetProperty("unresponsiveTreatmentAnimals").ValueKind);
    }

    [Fact]
    public async Task Dashboard_WithPeriodEndingBeforeStart_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var (from, to) = CurrentMonthPeriod();

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/dashboard?from={to.ToQueryValue()}&to={from.ToQueryValue()}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("DashboardAnalytics.InvalidPeriod", problem.GetProperty("code").GetString());
    }

    private static (DateTimeOffset From, DateTimeOffset To) CurrentMonthPeriod()
    {
        var now = DateTimeOffset.UtcNow;
        return (new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero), now.AddHours(1));
    }
}
