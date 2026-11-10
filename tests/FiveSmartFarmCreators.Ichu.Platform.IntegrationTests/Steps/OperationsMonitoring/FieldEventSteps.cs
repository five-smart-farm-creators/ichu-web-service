using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.OperationsMonitoring;

/// <summary>
///     Records field events and clinical interventions for cattle, and reads the livestock operations of cattle.
/// </summary>
[Binding]
public sealed class FieldEventSteps(ApiScenarioContext context)
{
    [When("the ranch manager records the field event {string} of type {string} for the cattle {string} with observation {string}")]
    public async Task WhenManagerRecordsFieldEvent(string alias, string type, string cattleAlias, string observation)
    {
        var operationId = RememberOperation(alias);
        await context.RecordAsync(context.Actor("manager").PostJsonAsync("api/v1/field-events", new
        {
            operationId,
            animalId = context.Id(cattleAlias),
            type,
            occurredAt = context.Time(alias),
            observation
        }));
    }

    [When("the ranch manager records a field event without its {word}")]
    public async Task WhenManagerRecordsFieldEventWithoutField(string missing)
    {
        var (cattleId, type) = missing switch
        {
            "cattle" => ((Guid?)null, (string?)"HealthEvent"),
            "type" => ((Guid?)context.Id("monitored cattle"), (string?)null),
            _ => throw new ArgumentOutOfRangeException(nameof(missing), missing, "Only the cattle or the type can be omitted.")
        };
        await context.RecordAsync(context.Actor("manager").PostJsonAsync("api/v1/field-events", new
        {
            operationId = Guid.NewGuid(),
            animalId = cattleId,
            type,
            occurredAt = MonitoringTime.Normalize(DateTimeOffset.UtcNow.AddHours(-1))
        }));
    }

    [When("the ranch manager records the clinical intervention {string} of type {string} for the cattle {string} with product {string} and dose {string}")]
    public async Task WhenManagerRecordsClinicalIntervention(string alias, string type, string cattleAlias, string product, string dose)
    {
        var operationId = RememberOperation(alias);
        await context.RecordAsync(context.Actor("manager").PostJsonAsync("api/v1/clinical-interventions", new
        {
            operationId,
            animalId = context.Id(cattleAlias),
            type,
            occurredAt = context.Time(alias),
            product,
            dose
        }));
    }

    [When("the ranch manager requests the livestock operations of the cattle {string} for the last {int} days")]
    public async Task WhenManagerRequestsOperations(string cattleAlias, int days) => await RequestOperationsAsync("manager", cattleAlias, days);

    [When("the veterinarian requests the livestock operations of the cattle {string} for the last {int} days")]
    public async Task WhenVeterinarianRequestsOperations(string cattleAlias, int days) => await RequestOperationsAsync("veterinarian", cattleAlias, days);

    [Then("the operation {string} has:")]
    public void ThenOperationHas(string alias, DataTable table) =>
        TableAssertions.AssertFields(context, TableAssertions.FindByText(context.RequireBody(), "operationId", context.Id(alias).ToString()), table);

    // The operation happened an hour ago, so the server accepts it and it falls inside the period of the query.
    private Guid RememberOperation(string alias)
    {
        var operationId = Guid.NewGuid();
        context.Remember(alias, operationId);
        context.RememberTime(alias, MonitoringTime.Normalize(DateTimeOffset.UtcNow.AddHours(-1)));
        return operationId;
    }

    private async Task RequestOperationsAsync(string actor, string cattleAlias, int days) =>
        await context.RecordAsync(context.Actor(actor)
            .GetAsync($"api/v1/animals/{context.Id(cattleAlias)}/livestock-operations?{ApiScenarioContext.LastDaysQuery(days)}"));
}
