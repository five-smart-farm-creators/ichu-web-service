using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Reqnroll;
using Xunit;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.OperationsMonitoring;

/// <summary>
///     Builds the readings and alerts that an edge gateway holds, synchronizes them, and checks the result of each entry.
/// </summary>
[Binding]
public sealed class EdgeSynchronizationSteps(ApiScenarioContext context)
{
    // The fever threshold configured by the harness, which the alerts of the scenarios cite.
    private const double FeverMinimum = 37.5;
    private const double FeverMaximum = 39.5;

    [Given("the edge gateway holds a reading {string} from the collar {string} with body temperature {float}")]
    public void GivenHoldsReading(string alias, string collarAlias, double temperature) =>
        HoldNewReading(alias, collarAlias, MonitoringTime.Normalize(DateTimeOffset.UtcNow), Metric(temperature));

    [Given("the edge gateway holds a reading {string} from the collar {string} with body temperature {float} captured {int} minutes from now")]
    public void GivenHoldsReadingCapturedAt(string alias, string collarAlias, double temperature, int minutes) =>
        HoldNewReading(alias, collarAlias, MonitoringTime.Normalize(DateTimeOffset.UtcNow.AddMinutes(minutes)), Metric(temperature));

    [Given("the edge gateway holds a reading {string} from the collar {string} with an invalid body temperature because {string}")]
    public void GivenHoldsReadingWithInvalidTemperature(string alias, string collarAlias, string reason) =>
        HoldNewReading(alias, collarAlias, MonitoringTime.Normalize(DateTimeOffset.UtcNow), Metric(null, reason));

    [Given("the edge gateway holds a reading {string} from the collar {string} at latitude {float} and longitude {float}")]
    public void GivenHoldsPositionReading(string alias, string collarAlias, double latitude, double longitude)
    {
        HoldNewReading(alias, collarAlias, MonitoringTime.Normalize(DateTimeOffset.UtcNow), null);
        context.PendingRecords.HeldReadings[alias].Position = new { latitude, longitude, accuracyMeters = 5.0, invalidReason = (string?)null };
    }

    [Given("the edge gateway holds a copy {string} of the reading {string}")]
    public void GivenHoldsCopy(string alias, string originalAlias) =>
        HoldCopy(alias, originalAlias, context.PendingRecords.HeldReadings[originalAlias].BodyTemperature);

    [Given("the edge gateway holds a copy {string} of the reading {string} with body temperature {float}")]
    public void GivenHoldsAlteredCopy(string alias, string originalAlias, double temperature) =>
        HoldCopy(alias, originalAlias, Metric(temperature));

    [Given("the reading {string} also reports rumination of {float} minutes")]
    public void GivenReadingReportsRumination(string alias, double minutes) =>
        context.PendingRecords.HeldReadings[alias].RuminationMinutes = Metric(minutes);

    [Given("the edge gateway holds a fever alert {string} citing the reading {string}")]
    public void GivenHoldsFeverAlert(string alias, string readingAlias)
    {
        var reading = context.PendingRecords.HeldReadings[readingAlias];
        var alertId = Guid.NewGuid();
        context.Remember(alias, alertId);
        context.PendingRecords.HoldAlert(new
        {
            alertId,
            deviceId = reading.DeviceId,
            type = "Temperature",
            severity = "Warning",
            readingIds = new[] { reading.ReadingId },
            configurationRevision = Math.Max(context.RuleRevision, 1),
            thresholdMinValue = (double?)FeverMinimum,
            thresholdMaxValue = (double?)FeverMaximum,
            geofenceId = (Guid?)null,
            distanceToBoundaryMeters = (double?)null
        });
    }

    [Given("the edge gateway holds a geofence alert {string} citing the reading {string} at {float} meters from the boundary of the geofence {string}")]
    public void GivenHoldsGeofenceAlert(string alias, string readingAlias, double distanceMeters, string geofenceName)
    {
        var reading = context.PendingRecords.HeldReadings[readingAlias];
        var alertId = Guid.NewGuid();
        context.Remember(alias, alertId);
        context.PendingRecords.HoldAlert(new
        {
            alertId,
            deviceId = reading.DeviceId,
            type = "Geofence",
            severity = "Critical",
            readingIds = new[] { reading.ReadingId },
            configurationRevision = Math.Max(context.RuleRevision, 1),
            thresholdMinValue = (double?)null,
            thresholdMaxValue = (double?)null,
            geofenceId = (Guid?)context.Id(geofenceName),
            distanceToBoundaryMeters = (double?)distanceMeters
        });
    }

    [When("the edge gateway records the return to the geofence of the alert {string} as {string}")]
    public void WhenRecordsReturn(string alertAlias, string operationAlias)
    {
        var operationId = Guid.NewGuid();
        var returnedAt = MonitoringTime.Normalize(DateTimeOffset.UtcNow);
        context.Remember(operationAlias, operationId);
        context.RememberTime(operationAlias, returnedAt);
        context.PendingRecords.HoldAlertChange(new { operationId, alertId = context.Id(alertAlias), kind = "Cleared", occurredAt = returnedAt });
    }

    [When("the edge gateway synchronizes its pending records")]
    public async Task WhenGatewaySynchronizesPendingRecords()
    {
        var batch = context.PendingRecords.TakeBatch();
        context.LastBatch = batch;
        await context.RecordAsync(context.Anonymous().PostJsonWithGatewayKeyAsync("api/v1/telemetry/batch", context.GatewayKey, batch));
    }

    [When("the edge gateway resends the last batch")]
    public async Task WhenGatewayResendsLastBatch() =>
        await context.RecordAsync(context.Anonymous().PostJsonWithGatewayKeyAsync("api/v1/telemetry/batch", context.GatewayKey,
            context.LastBatch ?? throw new InvalidOperationException("The edge gateway has not synchronized a batch yet.")));

    [Then("the entry {string} is {string}")]
    public void ThenEntryIs(string alias, string status) => Assert.Equal(status, StatusOf(FindEntry(alias)));

    [Then("the entry {string} is {string} with a reason containing {string}")]
    public void ThenEntryIsWithReason(string alias, string status, string fragment)
    {
        var entry = FindEntry(alias);
        Assert.Equal(status, StatusOf(entry));
        Assert.Contains(fragment, entry.GetProperty("reason").GetString());
    }

    [Then("the entry {string} is {string} and joins the alert {string}")]
    public void ThenEntryJoinsAlert(string alias, string status, string alertAlias)
    {
        var entry = FindEntry(alias);
        Assert.Equal(status, StatusOf(entry));
        Assert.Equal(context.Id(alertAlias), entry.GetProperty("relatedAlertId").GetGuid());
    }

    private void HoldNewReading(string alias, string collarAlias, DateTimeOffset capturedAt, object? bodyTemperature) =>
        Hold(alias, new HeldReading
        {
            ReadingId = Guid.NewGuid(),
            DeviceId = context.Id(collarAlias),
            CapturedAt = capturedAt,
            BodyTemperature = bodyTemperature
        });

    private void HoldCopy(string alias, string originalAlias, object? bodyTemperature) =>
        Hold(alias, context.PendingRecords.HeldReadings[originalAlias].CopyWith(bodyTemperature));

    private void Hold(string alias, HeldReading reading)
    {
        context.Remember(alias, reading.ReadingId);
        context.RememberTime(alias, reading.CapturedAt);
        context.PendingRecords.HoldReading(alias, reading);
    }

    private JsonElement FindEntry(string alias)
    {
        Assert.Equal(HttpStatusCode.OK, context.LastStatus);
        return TableAssertions.FindByText(context.RequireBody().GetProperty("results"), "id", context.Id(alias).ToString());
    }

    private static string StatusOf(JsonElement entry) => entry.GetProperty("status").GetString()!;

    private static object Metric(double? value, string? invalidReason = null) => new { value, invalidReason };
}
