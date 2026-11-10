using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Iam;

[Collection(ApiCollection.Name)]
public sealed class IamApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task RegisterManager_ReturnsCreatedAccountAndRanch()
    {
        var identity = TestIdentity.Create();
        using var client = factory.CreateClient(identity);

        using var response = await client.PostJsonAsync("api/v1/accounts", ManagerRequest());

        var body = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.EndsWith("/api/v1/accounts/me", response.Headers.Location!.ToString());
        var account = body.GetProperty("account");
        Assert.Equal(identity.Email, account.GetProperty("email").GetString());
        Assert.Equal("RanchManager", account.GetProperty("role").GetString());
        Assert.Equal("PendingVerification", account.GetProperty("status").GetString());
        Assert.NotEqual(Guid.Empty, body.GetProperty("ranchId").GetGuid());
    }

    [Fact]
    public async Task GetCurrentAccount_ForVerifiedManager_ReturnsRoleAndEmail()
    {
        var identity = TestIdentity.Create();
        using var client = factory.CreateClient(identity);
        await client.RegisterManagerAsync();

        using var response = await client.GetAsync("api/v1/accounts/me");

        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(identity.Email, body.GetProperty("email").GetString());
        Assert.Equal("RanchManager", body.GetProperty("role").GetString());
        Assert.Equal("Active", body.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetCurrentRanches_ListsRegisteredRanchWithManageScope()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync("api/v1/accounts/me/ranches");

        var ranches = await response.ReadJsonAsync(HttpStatusCode.OK);
        var ranch = Assert.Single(ranches.EnumerateArray());
        Assert.Equal(ranchId, ranch.GetProperty("ranchId").GetGuid());
        Assert.Equal("Manage", ranch.GetProperty("scope").GetString());
        Assert.Equal(JsonValueKind.Null, ranch.GetProperty("expiresAt").ValueKind);
    }

    [Fact]
    public async Task RegisterAccount_TwiceForSameIdentity_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync("api/v1/accounts", ManagerRequest());

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("An account already exists for this identity.", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetCurrentAccount_WithUnverifiedEmail_IsUnauthorized()
    {
        using var client = factory.CreateClient(TestIdentity.Create(emailVerified: false));
        using var registration = await client.PostJsonAsync("api/v1/accounts", ManagerRequest());
        await registration.ReadBodyAsync(HttpStatusCode.Created);

        using var response = await client.GetAsync("api/v1/accounts/me");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("An active verified account is required.", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task SignIn_WithCorrectPassword_ReturnsBearerTokenThatAuthenticates()
    {
        var identity = TestIdentity.Create();
        factory.Identity.Register(identity.Uid, identity.Email);
        using (var registered = factory.CreateClient(identity))
        {
            await registered.RegisterManagerAsync();
        }
        using var anonymous = factory.CreateClient();

        using var response = await anonymous.PostJsonAsync("api/v1/accounts/sessions",
            new { email = identity.Email, password = FakeIdentityProvider.CorrectPassword });

        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Bearer", body.GetProperty("tokenType").GetString());
        using var authenticated = factory.CreateClient();
        authenticated.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("idToken").GetString());
        using var me = await authenticated.GetAsync("api/v1/accounts/me");
        var account = await me.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(identity.Email, account.GetProperty("email").GetString());
    }

    [Fact]
    public async Task SignIn_WithWrongPassword_ReturnsUnauthorized()
    {
        var identity = TestIdentity.Create();
        factory.Identity.Register(identity.Uid, identity.Email);
        using (var registered = factory.CreateClient(identity))
        {
            await registered.RegisterManagerAsync();
        }
        using var anonymous = factory.CreateClient();

        using var response = await anonymous.PostJsonAsync("api/v1/accounts/sessions",
            new { email = identity.Email, password = "wrong-password" });

        await response.ReadBodyAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterAccount_TwiceForSameIdentity_ReturnsStableCodeAndSpanishTitle()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        await client.RegisterManagerAsync();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("es");

        using var response = await client.PostJsonAsync("api/v1/accounts", ManagerRequest());

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("Iam.AccountAlreadyExists", problem.GetProperty("code").GetString());
        Assert.Equal("Ya existe una cuenta para esta identidad.", problem.GetProperty("title").GetString());
    }

    [Fact]
    public async Task SignIn_WithWrongPassword_ReturnsInvalidCredentialsCode()
    {
        var identity = TestIdentity.Create();
        factory.Identity.Register(identity.Uid, identity.Email);
        using (var registered = factory.CreateClient(identity))
        {
            await registered.RegisterManagerAsync();
        }
        using var anonymous = factory.CreateClient();

        using var response = await anonymous.PostJsonAsync("api/v1/accounts/sessions",
            new { email = identity.Email, password = "wrong-password" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Unauthorized);
        Assert.Equal("Iam.InvalidCredentials", problem.GetProperty("code").GetString());
    }

    private static object ManagerRequest() => new
    {
        role = "RanchManager",
        ranchName = "Los Andes",
        region = "Cusco",
        district = "Urubamba",
        estimatedHeadCount = 120
    };
}
