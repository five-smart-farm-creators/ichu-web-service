using System.Globalization;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.SubscriptionPlans;

/// <summary>
///     Reads the plan catalog and the price estimates, and requests the subscriptions of a ranch.
/// </summary>
[Binding]
public sealed class SubscriptionSteps(ApiScenarioContext context)
{
    [When("a visitor lists the subscription plans")]
    public async Task WhenVisitorListsPlans() => await context.RecordAsync(context.Anonymous().GetAsync("api/v1/plans"));

    [When("a visitor requests a quote for the {string} plan with the head count {string}")]
    public async Task WhenVisitorRequestsQuote(string planName, string headCount) =>
        await context.RecordAsync(context.Anonymous()
            .GetAsync($"api/v1/plans/{context.Id(planName)}/quote?headCount={Uri.EscapeDataString(headCount)}"));

    [When("the ranch manager requests a subscription to the {string} plan for the head count {string} with terms {string} and an expected total of {float}")]
    public async Task WhenManagerRequestsSubscription(string planName, string headCount, string terms, double expectedTotal) =>
        await context.RecordAsync(context.Actor("manager").PostJsonAsync($"api/v1/ranches/{context.Id("ranch")}/subscriptions", new
        {
            planId = context.Id(planName),
            headCount = int.Parse(headCount, CultureInfo.InvariantCulture),
            expectedTotal = (decimal)expectedTotal,
            termsVersion = terms,
            operationId = Guid.NewGuid(),
            acquisitionSource = "bdd-scenario"
        }));

    [When("the ranch manager reads the subscription of the ranch")]
    public async Task WhenManagerReadsSubscription() =>
        await context.RecordAsync(context.Actor("manager").GetAsync($"api/v1/ranches/{context.Id("ranch")}/subscriptions"));

    [Then("the plan {string} has:")]
    public void ThenPlanHas(string planName, DataTable table) =>
        TableAssertions.AssertFields(context, TableAssertions.FindByText(context.RequireBody(), "name", planName), table);
}
