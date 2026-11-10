using System.Net;
using System.Net.Http.Headers;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Shared;

[Collection(ApiCollection.Name)]
public sealed class SharedApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task ProtectedRoute_WithoutToken_ReturnsUnauthorizedWithBearerChallenge()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"api/v1/ranches/{Guid.NewGuid()}/cattle");

        await response.ReadBodyAsync(HttpStatusCode.Unauthorized);
        Assert.Contains(response.Headers.WwwAuthenticate, challenge => challenge.Scheme == "Bearer");
    }

    [Fact]
    public async Task ProtectedRoute_WithMalformedToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-an-identity-token");

        using var response = await client.GetAsync($"api/v1/ranches/{Guid.NewGuid()}/cattle");

        await response.ReadBodyAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidationFailure_WithSpanishAcceptLanguage_ReturnsLocalizedTitleAndStableCode()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es");

        // An undefined breed passes model binding, so the domain rejects it and the assembler localizes the answer.
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/cattle", new
        {
            earTag = "PE-0100",
            breed = 99,
            sex = 1,
            birthDate = new DateOnly(2024, 3, 1),
            lifeStage = 2
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("La raza no es válida.", problem.GetProperty("title").GetString());
        Assert.Equal("CattleInformation.InvalidBreed", problem.GetProperty("code").GetString());
    }
}
