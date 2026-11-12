using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.OperationsMonitoring;

/// <summary>
///     Records the breeding services, pregnancy diagnoses and calvings of cattle, and reads the reproductive history of an animal.
/// </summary>
[Binding]
public sealed class ReproductiveHistorySteps(ApiScenarioContext context)
{
    [When("the ranch manager records the breeding service {string} for the cattle {string} {int} days ago with sire {string}")]
    public async Task WhenManagerRecordsBreedingService(string alias, string cattleAlias, int daysAgo, string sire) =>
        await RecordAsync(alias, "clinical-interventions", "Insemination", cattleAlias, daysAgo, sireReference: sire);

    [When("the ranch manager records the pregnancy diagnosis {string} for the cattle {string} {int} days ago with result {string}")]
    public async Task WhenManagerRecordsPregnancyDiagnosis(string alias, string cattleAlias, int daysAgo, string result) =>
        await RecordAsync(alias, "clinical-interventions", "PregnancyDiagnosis", cattleAlias, daysAgo, pregnancyResult: result);

    [When("the ranch manager records the calving {string} for the cattle {string} {int} days ago")]
    public async Task WhenManagerRecordsCalving(string alias, string cattleAlias, int daysAgo) =>
        await RecordAsync(alias, "field-events", "Birth", cattleAlias, daysAgo);

    [When("the veterinarian requests the reproductive history of the cattle {string}")]
    public async Task WhenVeterinarianRequestsReproductiveHistory(string cattleAlias) =>
        await context.RecordAsync(context.Actor("veterinarian").GetAsync(
            $"api/v1/ranches/{context.Id("ranch")}/cattle/{context.Id(cattleAlias)}/reproductive-history"));

    private async Task RecordAsync(
        string alias,
        string route,
        string type,
        string cattleAlias,
        int daysAgo,
        string? sireReference = null,
        string? pregnancyResult = null)
    {
        var operationId = Guid.NewGuid();
        var occurredAt = MonitoringTime.Normalize(DateTimeOffset.UtcNow.AddDays(-daysAgo));
        context.Remember(alias, operationId);
        context.RememberTime(alias, occurredAt);
        await context.RecordAsync(context.Actor("manager").PostJsonAsync($"api/v1/ranches/{context.Id("ranch")}/{route}", new
        {
            operationId,
            cattleId = context.Id(cattleAlias),
            type,
            occurredAt,
            sireReference,
            pregnancyResult
        }));
    }
}
