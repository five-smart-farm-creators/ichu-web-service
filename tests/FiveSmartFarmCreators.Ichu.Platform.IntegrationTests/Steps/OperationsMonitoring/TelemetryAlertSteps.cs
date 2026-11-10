using System.Text.Json;
using Reqnroll;
using Xunit;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.OperationsMonitoring;

/// <summary>
///     Reads the telemetry readings and the alerts of a ranch, as a ranch manager or a veterinarian.
/// </summary>
[Binding]
public sealed class TelemetryAlertSteps(ApiScenarioContext context)
{
    [When("the ranch manager requests the telemetry of the cattle {string} for the last {int} days")]
    public async Task WhenManagerRequestsTelemetry(string cattleAlias, int days) => await RequestTelemetryAsync("manager", cattleAlias, days);

    [When("the veterinarian requests the telemetry of the cattle {string} for the last {int} days")]
    public async Task WhenVeterinarianRequestsTelemetry(string cattleAlias, int days) => await RequestTelemetryAsync("veterinarian", cattleAlias, days);

    [When("the ranch manager lists the alerts of the ranch")]
    public async Task WhenManagerListsAlerts() => await ListAlertsAsync("manager");

    [When("the veterinarian lists the alerts of the ranch")]
    public async Task WhenVeterinarianListsAlerts() => await ListAlertsAsync("veterinarian");

    [Then("the telemetry item {string} has:")]
    public void ThenTelemetryItemHas(string readingAlias, DataTable table) =>
        TableAssertions.AssertFields(context,
            TableAssertions.FindByText(context.RequireBody().GetProperty("items"), "readingId", context.Id(readingAlias).ToString()), table);

    [Then("the alert {string} has:")]
    public void ThenAlertHas(string alertAlias, DataTable table) => TableAssertions.AssertFields(context, FindAlert(alertAlias), table);

    [Then("the exit duration of the alert {string} equals the time from its raising to its return")]
    public void ThenExitDurationMatches(string alertAlias)
    {
        var alert = FindAlert(alertAlias);
        var elapsed = alert.GetProperty("clearedAt").GetDateTimeOffset() - alert.GetProperty("raisedAt").GetDateTimeOffset();
        Assert.Equal(elapsed.TotalSeconds, alert.GetProperty("exitDurationSeconds").GetDouble(), precision: 3);
    }

    private async Task RequestTelemetryAsync(string actor, string cattleAlias, int days) =>
        await context.RecordAsync(context.Actor(actor)
            .GetAsync($"api/v1/ranches/{context.Id("ranch")}/cattle/{context.Id(cattleAlias)}/telemetry?{ApiScenarioContext.LastDaysQuery(days)}"));

    private async Task ListAlertsAsync(string actor) =>
        await context.RecordAsync(context.Actor(actor).GetAsync($"api/v1/ranches/{context.Id("ranch")}/alerts"));

    private JsonElement FindAlert(string alias) =>
        TableAssertions.FindByText(context.RequireBody().GetProperty("items"), "alertId", context.Id(alias).ToString());
}
