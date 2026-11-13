using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Iam;

[Collection(ApiCollection.Name)]
public sealed class VeterinaryAccessRequestsApiTests(IchuApiFactory factory)
{
    private const string RequestsPath = "api/v1/veterinary-access-requests";

    [Fact]
    public async Task FindRanchThenRequestAccess_ReturnsPendingWithoutGrantingRanchAccess()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        using var ranchesResponse = await owner.GetAsync("api/v1/accounts/me/ranches");
        var ownedRanch = Assert.Single((await ranchesResponse.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        var accessCode = ownedRanch.GetProperty("accessCode").GetString();
        using var lookup = await veterinarian.GetAsync($"api/v1/ranches?accessCode={accessCode}");
        var ranch = await lookup.ReadJsonAsync(HttpStatusCode.OK);

        using var response = await veterinarian.PostJsonAsync(RequestsPath, new
        {
            ranchId = ranch.GetProperty("ranchId").GetGuid(),
            reason = "Herd health review"
        });

        var request = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal("Pending", request.GetProperty("status").GetString());
        Assert.Equal(ranchId, request.GetProperty("ranchId").GetGuid());
        Assert.Equal($"/{RequestsPath}?ranchId={ranchId}", response.Headers.Location!.ToString());
        Assert.Equal(JsonValueKind.Null, request.GetProperty("grantedAt").ValueKind);
        using var accessibleRanches = await veterinarian.GetAsync("api/v1/accounts/me/ranches");
        Assert.Empty((await accessibleRanches.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        var listed = Assert.Single((await ListAsync(veterinarian, ranchId)).EnumerateArray());
        Assert.Equal(request.GetProperty("id").GetGuid(), listed.GetProperty("id").GetGuid());
        Assert.Equal("Pending", listed.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("grant", "Granted")]
    [InlineData("reject", "Rejected")]
    public async Task DecideRequest_ByTheRanchOwner_UpdatesTheRequestAndVeterinarianAccess(string decision, string expectedStatus)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var requestId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, ranchId);
        var expiresAt = DateTimeOffset.UtcNow.AddDays(1);

        using var response = await owner.PatchJsonAsync($"{RequestsPath}/{requestId}", new { decision, expiresAt });

        var request = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal(requestId, request.GetProperty("id").GetGuid());
        Assert.Equal(expectedStatus, request.GetProperty("status").GetString());
        var listed = Assert.Single((await ListAsync(veterinarian, ranchId)).EnumerateArray());
        Assert.Equal(expectedStatus, listed.GetProperty("status").GetString());
        using var accessibleRanches = await veterinarian.GetAsync("api/v1/accounts/me/ranches");
        var ranches = await accessibleRanches.ReadJsonAsync(HttpStatusCode.OK);
        if (decision == "grant")
        {
            Assert.Equal("Read", request.GetProperty("scope").GetString());
            Assert.Equal(expiresAt, request.GetProperty("expiresAt").GetDateTimeOffset());
            Assert.Equal(ranchId, Assert.Single(ranches.EnumerateArray()).GetProperty("ranchId").GetGuid());
        }
        else
        {
            Assert.NotEqual(JsonValueKind.Null, request.GetProperty("endedAt").ValueKind);
            Assert.Empty(ranches.EnumerateArray());
        }
    }

    [Theory]
    [InlineData("another owner", "grant")]
    [InlineData("another owner", "reject")]
    [InlineData("requesting veterinarian", "grant")]
    [InlineData("requesting veterinarian", "reject")]
    [InlineData("another veterinarian", "grant")]
    [InlineData("another veterinarian", "reject")]
    public async Task DecideRequest_ByAnAccountOtherThanTheRanchOwner_IsForbidden(string actor, string decision)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var requestId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, ranchId);
        using var anotherOwner = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await anotherOwner.RegisterManagerAsync();
        using var anotherVeterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(anotherVeterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(anotherVeterinarian);
        var caller = actor switch
        {
            "another owner" => anotherOwner,
            "requesting veterinarian" => veterinarian,
            _ => anotherVeterinarian
        };

        using var response = await caller.PatchJsonAsync($"{RequestsPath}/{requestId}", new
        {
            decision,
            ranchId = otherRanchId,
            expiresAt = DateTimeOffset.UtcNow.AddDays(1)
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Iam.OwnerRequired", problem.GetProperty("code").GetString());
        Assert.Equal("Pending", Assert.Single((await ListAsync(owner, ranchId)).EnumerateArray()).GetProperty("status").GetString());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RequestAccess_WhenAPendingRequestOrActiveAccessExists_ReturnsTheExistingConflict(bool grantFirst)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var requestId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, ranchId);
        if (grantFirst)
        {
            await Arrange.GrantReadAdvisoryAsync(owner, ranchId, requestId, DateTimeOffset.UtcNow.AddDays(1));
        }

        using var response = await veterinarian.PostJsonAsync(RequestsPath, new { ranchId, reason = "Another review" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("Iam.AdvisoryRequestAlreadyOpen", problem.GetProperty("code").GetString());
        var stored = Assert.Single((await ListAsync(owner, ranchId)).EnumerateArray());
        Assert.Equal(requestId, stored.GetProperty("id").GetGuid());
        Assert.Equal(grantFirst ? "Granted" : "Pending", stored.GetProperty("status").GetString());
    }

    [Fact]
    public async Task RequestAccess_WithoutACompleteVeterinarianProfile_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);

        using var response = await veterinarian.PostJsonAsync(RequestsPath, new { ranchId, reason = "Herd health review" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Iam.VeterinarianProfileRequired", problem.GetProperty("code").GetString());
        Assert.Empty((await ListAsync(owner, ranchId)).EnumerateArray());
    }

    [Fact]
    public async Task RequestAccess_ByARanchManager_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();

        using var response = await owner.PostJsonAsync(RequestsPath, new { ranchId, reason = "Herd health review" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Iam.VeterinarianProfileRequired", problem.GetProperty("code").GetString());
    }

    [Theory]
    [InlineData(false, "Herd health review")]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    public async Task RequestAccess_WithMissingRanchOrReason_IsBadRequest(bool includeRanch, string reason)
    {
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);

        using var response = await veterinarian.PostJsonAsync(RequestsPath, new { ranchId = includeRanch ? Guid.NewGuid() : (Guid?)null, reason });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListRequests_ShowsAllToTheOwnerAndOnlyTheVeterinariansOwnRequests()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var firstVeterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(firstVeterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(firstVeterinarian);
        var firstId = await Arrange.RequestHerdAdvisoryAsync(firstVeterinarian, ranchId);
        using var secondVeterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(secondVeterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(secondVeterinarian);
        var secondId = await Arrange.RequestHerdAdvisoryAsync(secondVeterinarian, ranchId);

        var ownersRequests = await ListAsync(owner, ranchId);
        var firstRequests = await ListAsync(firstVeterinarian, ranchId);
        var secondRequests = await ListAsync(secondVeterinarian, ranchId);

        Assert.Equal(2, ownersRequests.GetArrayLength());
        Assert.All(ownersRequests.EnumerateArray(), request =>
            Assert.Equal("Bovine health", request.GetProperty("veterinarianProfile").GetProperty("specialty").GetString()));
        Assert.Equal(firstId, Assert.Single(firstRequests.EnumerateArray()).GetProperty("id").GetGuid());
        Assert.Equal(secondId, Assert.Single(secondRequests.EnumerateArray()).GetProperty("id").GetGuid());
        using var anotherOwner = factory.CreateClient(TestIdentity.Create());
        await anotherOwner.RegisterManagerAsync();
        using var forbidden = await anotherOwner.GetAsync($"{RequestsPath}?ranchId={ranchId}");
        var problem = await forbidden.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Iam.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task LegacyRequest_CanBeDecidedAndListedThroughTheExplicitRoutes()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        using var legacyRequest = await veterinarian.PostJsonAsync($"api/v1/ranches/{ranchId}/herd-advisories", new { reason = "Herd health review" });
        var requestId = (await legacyRequest.ReadJsonAsync(HttpStatusCode.Created)).GetProperty("id").GetGuid();

        await Arrange.GrantReadAdvisoryAsync(owner, ranchId, requestId, DateTimeOffset.UtcNow.AddDays(1));

        var request = Assert.Single((await ListAsync(owner, ranchId)).EnumerateArray());
        Assert.Equal(requestId, request.GetProperty("id").GetGuid());
        Assert.Equal("Granted", request.GetProperty("status").GetString());
        using var legacyList = await owner.GetAsync($"api/v1/ranches/{ranchId}/herd-advisories");
        Assert.Equal(request.GetRawText(), Assert.Single((await legacyList.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray()).GetRawText());
    }

    [Fact]
    public async Task DirectGrant_StillSupportsOwnerAuthorizationWithoutAPriorRequest()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var veterinarianId = await veterinarian.GetCurrentAccountIdAsync();
        var grant = new { veterinarianId, scope = "Read", expiresAt = DateTimeOffset.UtcNow.AddDays(1) };
        Assert.Empty((await ListAsync(owner, ranchId)).EnumerateArray());

        using var response = await owner.PostJsonAsync($"api/v1/ranches/{ranchId}/herd-advisories/grants", grant);

        var request = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal("Granted", request.GetProperty("status").GetString());
        Assert.Equal("Direct authorization by the ranch owner.", request.GetProperty("reason").GetString());
        var listed = Assert.Single((await ListAsync(veterinarian, ranchId)).EnumerateArray());
        Assert.Equal(request.GetProperty("id").GetGuid(), listed.GetProperty("id").GetGuid());
        using var repeated = await owner.PostJsonAsync($"api/v1/ranches/{ranchId}/herd-advisories/grants", grant);
        var problem = await repeated.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("Iam.AdvisoryAlreadyActive", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DecideRequest_ForAnUnknownRequest_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        await owner.RegisterManagerAsync();

        using var response = await owner.PatchJsonAsync($"{RequestsPath}/{Guid.NewGuid()}", new { decision = "reject" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Iam.AdvisoryNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ListRequests_WithoutRanchId_IsBadRequest()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        await owner.RegisterManagerAsync();

        using var response = await owner.GetAsync(RequestsPath);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AccessRequests_WithoutAuthentication_AreUnauthorized()
    {
        using var anonymous = factory.CreateClient();

        using var request = await anonymous.PostJsonAsync(RequestsPath, new { ranchId = Guid.NewGuid(), reason = "Herd health review" });
        using var decision = await anonymous.PatchJsonAsync($"{RequestsPath}/{Guid.NewGuid()}", new { decision = "reject" });
        using var list = await anonymous.GetAsync($"{RequestsPath}?ranchId={Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, request.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, decision.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, list.StatusCode);
    }

    private static async Task<JsonElement> ListAsync(HttpClient client, Guid ranchId)
    {
        using var response = await client.GetAsync($"{RequestsPath}?ranchId={ranchId}");
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }
}
