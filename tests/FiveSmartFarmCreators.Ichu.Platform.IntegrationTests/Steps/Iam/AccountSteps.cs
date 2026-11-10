using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Reqnroll;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Steps.Iam;

/// <summary>
///     Sets up the ranch manager and the veterinarian authorization that the Identity and Access rules require before monitoring or subscribing.
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
        await Arrange.GrantReadAdvisoryAsync(context.Actor("manager"), advisoryId, DateTimeOffset.UtcNow.AddDays(1));
        context.Remember("veterinarian advisory", advisoryId);
    }

    [When("the ranch owner revokes the authorization of the veterinarian")]
    public async Task WhenOwnerRevokesVeterinarianAuthorization() =>
        await Arrange.RevokeAdvisoryAsync(context.Actor("manager"), context.Id("veterinarian advisory"));
}
