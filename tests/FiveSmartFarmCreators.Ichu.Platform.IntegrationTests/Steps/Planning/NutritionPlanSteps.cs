using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.Planning;

/// <summary>
///     Registers the nutrition plan of a lot, and reads the plan in force for a lot on a date.
/// </summary>
[Binding]
public sealed class NutritionPlanSteps(ApiScenarioContext context)
{
    [When("the ranch manager registers the nutrition plan {string} for the lot {string} valid from {string} with the feed {string} of kind {string} at {float} kilograms per head per day")]
    public async Task WhenManagerRegistersNutritionPlan(string name, string lotName, string validFrom, string feed, string kind, double kilograms) =>
        await context.RecordAsync(context.Actor("manager").PostJsonAsync(
            $"api/v1/lots/{context.Id(lotName)}/nutrition-plans", new
            {
                name,
                validFrom,
                items = new[] { new { feed, kind, kilogramsPerHeadPerDay = kilograms } }
            }));

    [When("the veterinarian requests the nutrition plan of the lot {string} in force on {string}")]
    public async Task WhenVeterinarianRequestsCurrentPlan(string lotName, string day) =>
        await context.RecordAsync(context.Actor("veterinarian").GetAsync(
            $"api/v1/lots/{context.Id(lotName)}/nutrition-plans/current?on={day}"));
}
