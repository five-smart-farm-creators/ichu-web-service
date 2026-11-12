using System.Net;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.Iam;

/// <summary>
///     Sets up the ranch manager and veterinarian accounts, and drives the access-code lookup and the advisory requests of the Identity and Access rules.
/// </summary>
[Binding]
public sealed class AccountSteps(ApiScenarioContext context)
{
    [Given("a verified ranch manager with a ranch")]
    public async Task GivenVerifiedRanchManagerWithRanch() => await ScenarioSetup.RegisterRanchManagerAsync(context);

    [Given("a veterinarian with a valid authorization over the ranch")]
    public async Task GivenVeterinarianWithValidAuthorization()
    {
        context.SignIn("veterinarian", TestIdentity.Create());
        var veterinarian = context.Actor("veterinarian");
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var advisoryId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, context.Id("ranch"));
        await Arrange.GrantReadAdvisoryAsync(context.Actor("manager"), context.Id("ranch"), advisoryId, DateTimeOffset.UtcNow.AddDays(1));
        context.Remember("veterinarian advisory", advisoryId);
    }

    [When("the ranch owner revokes the authorization of the veterinarian")]
    public async Task WhenOwnerRevokesVeterinarianAuthorization() =>
        await Arrange.RevokeAdvisoryAsync(context.Actor("manager"), context.Id("ranch"), context.Id("veterinarian advisory"));

    [Given("a veterinarian with a complete profile")]
    public async Task GivenVeterinarianWithCompleteProfile()
    {
        context.SignIn("veterinarian", TestIdentity.Create());
        var veterinarian = context.Actor("veterinarian");
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        context.Remember("veterinarian account", await veterinarian.GetCurrentAccountIdAsync());
    }

    [When("the veterinarian looks up the ranch with the access code of the ranch manager")]
    public async Task WhenVeterinarianLooksUpRanchByCode()
    {
        var accessCode = await ManagerAccessCodeAsync();
        await context.RecordAsync(context.Actor("veterinarian").GetAsync($"api/v1/ranches?accessCode={accessCode}"));
    }

    [When("the veterinarian requests advisory access to the ranch found by its code")]
    public async Task WhenVeterinarianRequestsAdvisoryAccess()
    {
        var ranchId = context.RequireBody().GetProperty("ranchId").GetGuid();
        await context.RecordAsync(context.Actor("veterinarian").PostJsonAsync($"api/v1/ranches/{ranchId}/herd-advisories", new
        {
            reason = "Herd health review"
        }));
    }

    [When("the ranch manager lists the herd advisories of the ranch")]
    public async Task WhenManagerListsHerdAdvisories() =>
        await context.RecordAsync(context.Actor("manager").GetAsync($"api/v1/ranches/{context.Id("ranch")}/herd-advisories"));

    [Then("the herd advisory of the veterinarian has:")]
    public void ThenHerdAdvisoryOfVeterinarianHas(DataTable table) =>
        TableAssertions.AssertFields(context,
            TableAssertions.FindByText(context.RequireBody(), "veterinarianId", context.Id("veterinarian account").ToString()), table);

    // The manager reads the code from its own ranch list, as it would before sharing the code.
    private async Task<string> ManagerAccessCodeAsync()
    {
        using var response = await context.Actor("manager").GetAsync("api/v1/accounts/me/ranches");
        var ranches = await response.ReadJsonAsync(HttpStatusCode.OK);
        return TableAssertions.FindByText(ranches, "ranchId", context.Id("ranch").ToString()).GetProperty("accessCode").GetString()!;
    }
}
