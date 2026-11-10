using System.Globalization;
using System.Net;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;
using Xunit;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.OperationsMonitoring;

/// <summary>
///     Sets up cattle, collars, lots, geofences and the fever threshold, and reads the rules that edge gateways receive.
/// </summary>
[Binding]
public sealed class MonitoringSetupSteps(ApiScenarioContext context)
{
    // A square around the lot, so that scenarios which do not test the boundary still define a valid geofence.
    private static readonly (double Latitude, double Longitude)[] SquareBoundary =
    [
        (-13.50, -72.10),
        (-13.50, -72.09),
        (-13.49, -72.09),
        (-13.49, -72.10)
    ];

    [Given("a ranch manager with monitored cattle")]
    public async Task GivenRanchManagerWithMonitoredCattle()
    {
        await ScenarioSetup.RegisterRanchManagerAsync(context);
        await ScenarioSetup.MonitorCattleAsync(context);
    }

    [Given("the ranch has the fever threshold configured")]
    public async Task GivenFeverThresholdConfigured()
    {
        var threshold = await Arrange.ConfigureFeverThresholdAsync(context.Actor("manager"), context.Id("ranch"));
        RememberRevision(threshold.GetProperty("revision").GetInt64());
    }

    [Given("a lot {string} exists in the ranch")]
    public async Task GivenLotExists(string name)
    {
        var lot = await Arrange.CreateLotAsync(context.Actor("manager"), context.Id("ranch"), name);
        context.Remember(name, lot.GetProperty("id").GetGuid());
        context.RememberVersion(name, lot.GetProperty("version").GetGuid());
    }

    [Given("the cattle {string} belongs to the lot {string}")]
    public async Task GivenCattleBelongsToLot(string cattleAlias, string lotName) =>
        await Arrange.AssignToLotAsync(context.Actor("manager"), context.Id("ranch"), context.Id(lotName), context.Id(cattleAlias),
            context.Version(lotName));

    [Given("the lot {string} has the geofence {string}")]
    public async Task GivenLotHasGeofence(string lotName, string geofenceName)
    {
        await context.RecordAsync(PostGeofenceAsync(geofenceName, lotName, SquareBoundary));
        Assert.Equal(HttpStatusCode.Created, context.LastStatus);
        RememberGeofence(geofenceName);
    }

    [When("the ranch manager defines the geofence {string} for the lot {string} with the boundary:")]
    public async Task WhenManagerDefinesGeofence(string geofenceName, string lotName, DataTable boundary)
    {
        var vertices = boundary.Rows.Select(row =>
                (Latitude: double.Parse(row["latitude"], CultureInfo.InvariantCulture),
                 Longitude: double.Parse(row["longitude"], CultureInfo.InvariantCulture)))
            .ToArray();
        await context.RecordAsync(PostGeofenceAsync(geofenceName, lotName, vertices));
        if (context.LastStatus == HttpStatusCode.Created)
        {
            RememberGeofence(geofenceName);
        }
    }

    [When("the ranch manager lists the geofences of the ranch")]
    public async Task WhenManagerListsGeofences() =>
        await context.RecordAsync(context.Actor("manager").GetAsync($"api/v1/geofences?ranchId={context.Id("ranch")}"));

    [Given("a collar {string} is registered without an assignment")]
    public async Task GivenCollarRegisteredWithoutAssignment(string alias)
    {
        var collar = await Arrange.RegisterCollarAsync(context.Actor("manager"), context.Id("ranch"), $"COL-{Guid.NewGuid():N}");
        context.Remember(alias, collar.GetProperty("id").GetGuid());
    }

    [Given("a collar {string} is registered in another ranch")]
    public async Task GivenCollarRegisteredInAnotherRanch(string alias) =>
        context.Remember(alias, await ScenarioSetup.RegisterCollarInAnotherRanchAsync(context));

    [Given("cattle {string} is registered in the ranch")]
    public async Task GivenCattleRegistered(string alias)
    {
        // Ear tags must be unique within a ranch, so each cattle gets a random one.
        var earTag = $"PE-{Guid.NewGuid():N}"[..11];
        var cattle = await Arrange.RegisterCattleAsync(context.Actor("manager"), context.Id("ranch"), earTag);
        context.Remember(alias, cattle.GetProperty("id").GetGuid());
    }

    [When("the ranch manager releases the collar {string}")]
    public async Task WhenManagerReleasesCollar(string alias)
    {
        var device = await Arrange.ReleaseCollarAsync(context.Actor("manager"), context.Id("ranch"), context.Id(alias), context.Version(alias));
        context.RememberVersion(alias, device.GetProperty("version").GetGuid());
    }

    [When("the ranch manager assigns the collar {string} to the cattle {string}")]
    public async Task WhenManagerAssignsCollar(string collarAlias, string cattleAlias)
    {
        var device = await Arrange.AssignCollarAsync(context.Actor("manager"), context.Id("ranch"), context.Id(collarAlias),
            context.Id(cattleAlias), context.Version(collarAlias));
        context.RememberVersion(collarAlias, device.GetProperty("version").GetGuid());
    }

    [When("the edge gateway reads its monitoring rules")]
    public async Task WhenGatewayReadsMonitoringRules() =>
        await context.RecordAsync(context.Anonymous().GetWithGatewayKeyAsync(
            $"api/v1/edge-gateways/{context.Id("gateway")}/monitoring-rules?afterRevision=0", context.GatewayKey));

    [Then("the monitoring rules include the geofence {string}")]
    public void ThenMonitoringRulesIncludeGeofence(string geofenceName)
    {
        var names = context.RequireBody().GetProperty("geofences").EnumerateArray().Select(geofence => geofence.GetProperty("name").GetString());
        Assert.Contains(geofenceName, names);
    }

    private Task<HttpResponseMessage> PostGeofenceAsync(string geofenceName, string lotName, IEnumerable<(double Latitude, double Longitude)> boundary) =>
        context.Actor("manager").PostJsonAsync("api/v1/geofences", new
        {
            ranchId = context.Id("ranch"),
            lotId = context.Id(lotName),
            name = geofenceName,
            toleranceMeters = 10,
            severity = "Critical",
            vertices = boundary.Select(vertex => new { latitude = vertex.Latitude, longitude = vertex.Longitude }).ToList()
        });

    private void RememberGeofence(string geofenceName)
    {
        var geofence = context.RequireBody();
        context.Remember(geofenceName, geofence.GetProperty("geofenceId").GetGuid());
        RememberRevision(geofence.GetProperty("revision").GetInt64());
    }

    private void RememberRevision(long revision) => context.RuleRevision = Math.Max(context.RuleRevision, revision);
}
