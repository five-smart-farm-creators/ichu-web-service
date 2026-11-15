using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using Microsoft.Extensions.Configuration;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.CattleInformation;

[Collection(ApiCollection.Name)]
public sealed class LotsApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task CreateLot_Returns201AndCreatesExactlyOneEmptyLotWithTheGroupingReason()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await Arrange.RegisterCattleAsync(owner, ranchId, "PE-LACTATION", lifeStage: 3);
        Assert.Empty((await ListLotsAsync(owner, ranchId)).EnumerateArray());

        using var response = await owner.PostJsonAsync(LotsPath(ranchId), new
        {
            name = "  Milking group  ",
            criterion = "  Animals in lactation  "
        });

        var created = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal($"/{LotsPath(ranchId)}", response.Headers.Location!.ToString());
        Assert.Equal(ranchId, created.GetProperty("ranchId").GetGuid());
        Assert.Equal("Milking group", created.GetProperty("name").GetString());
        Assert.Equal("Animals in lactation", created.GetProperty("criterion").GetString());
        Assert.NotEqual(Guid.Empty, created.GetProperty("id").GetGuid());
        Assert.NotEqual(Guid.Empty, created.GetProperty("version").GetGuid());
        Assert.Empty(created.GetProperty("memberships").EnumerateArray());
        var stored = Assert.Single((await ListLotsAsync(owner, ranchId)).EnumerateArray());
        Assert.Equal(created.GetRawText(), stored.GetRawText());
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, "   ")]
    public async Task CreateLot_WithAnOmittedOrBlankCriterion_StoresNull(bool includeCriterion, string? criterion)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var request = new Dictionary<string, object?> { ["name"] = "Milking group" };
        if (includeCriterion)
        {
            request["criterion"] = criterion;
        }

        using var response = await owner.PostJsonAsync(LotsPath(ranchId), request);

        var created = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(JsonValueKind.Null, created.GetProperty("criterion").ValueKind);
        var stored = Assert.Single((await ListLotsAsync(owner, ranchId)).EnumerateArray());
        Assert.Equal(created.GetProperty("id").GetGuid(), stored.GetProperty("id").GetGuid());
        Assert.Equal(JsonValueKind.Null, stored.GetProperty("criterion").ValueKind);
    }

    [Theory]
    [InlineData(120, HttpStatusCode.Created)]
    [InlineData(121, HttpStatusCode.BadRequest)]
    public async Task CreateLot_EnforcesTheCriterionLengthLimit(int length, HttpStatusCode expectedStatus)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var criterion = new string('a', length);

        using var response = await owner.PostJsonAsync(LotsPath(ranchId), new { name = "Milking group", criterion });

        Assert.Equal(expectedStatus, response.StatusCode);
        var lots = await ListLotsAsync(owner, ranchId);
        if (expectedStatus == HttpStatusCode.Created)
        {
            Assert.Equal(criterion, Assert.Single(lots.EnumerateArray()).GetProperty("criterion").GetString());
        }
        else
        {
            Assert.Empty(lots.EnumerateArray());
        }
    }

    [Fact]
    public async Task GetLots_ReturnsTheRanchCollectionAndMembershipsInOneResponse()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var lastLot = await Arrange.CreateLotAsync(owner, ranchId, "Zulu group");
        var firstLot = await Arrange.CreateLotAsync(owner, ranchId, "Alpha group");
        var cattle = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-LOT-MEMBER");
        await Arrange.AssignToLotAsync(owner, ranchId, lastLot.GetProperty("id").GetGuid(),
            cattle.GetProperty("id").GetGuid(), lastLot.GetProperty("version").GetGuid());
        using var otherOwner = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await otherOwner.RegisterManagerAsync();
        await Arrange.CreateLotAsync(otherOwner, otherRanchId, "Foreign group");

        using var response = await owner.GetAsync(LotsPath(ranchId));

        var lots = (await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray().ToList();
        Assert.Equal(2, lots.Count);
        Assert.Equal(firstLot.GetProperty("id").GetGuid(), lots[0].GetProperty("id").GetGuid());
        Assert.Equal(lastLot.GetProperty("id").GetGuid(), lots[1].GetProperty("id").GetGuid());
        Assert.All(lots, lot => Assert.Equal(ranchId, lot.GetProperty("ranchId").GetGuid()));
        Assert.Empty(lots[0].GetProperty("memberships").EnumerateArray());
        var member = Assert.Single(lots[1].GetProperty("memberships").EnumerateArray());
        Assert.Equal(cattle.GetProperty("id").GetGuid(), member.GetProperty("cattleId").GetGuid());
        Assert.Equal("Weight", lots[1].GetProperty("criterion").GetString());
    }

    [Fact]
    public async Task GetLots_WhenTheRanchHasNoLots_ReturnsAnEmptyCollection()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();

        using var response = await owner.GetAsync(LotsPath(ranchId));

        Assert.Empty((await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
    }

    [Fact]
    public async Task CreateLot_ByAnAccountWithoutRanchAccess_IsForbiddenAndCreatesNothing()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PostJsonAsync(LotsPath(ranchId), new { name = "Unauthorized lot" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("CattleInformation.RanchAccessDenied", problem.GetProperty("code").GetString());
        Assert.Empty((await ListLotsAsync(owner, ranchId)).EnumerateArray());
    }

    [Fact]
    public async Task GetLots_ByAnAccountWithoutRanchAccess_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await Arrange.CreateLotAsync(owner, ranchId, "Private lot");
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync(LotsPath(ranchId));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("CattleInformation.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task VeterinarianWithReadAccess_CanListLotsButCannotCreateThem()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var lot = await Arrange.CreateLotAsync(owner, ranchId, "Milking group");
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var requestId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, ranchId);
        await Arrange.GrantReadAdvisoryAsync(owner, ranchId, requestId, DateTimeOffset.UtcNow.AddDays(1));

        var listed = Assert.Single((await ListLotsAsync(veterinarian, ranchId)).EnumerateArray());
        Assert.Equal(lot.GetProperty("id").GetGuid(), listed.GetProperty("id").GetGuid());
        using var response = await veterinarian.PostJsonAsync(LotsPath(ranchId), new { name = "Unauthorized lot" });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("CattleInformation.RanchAccessDenied", problem.GetProperty("code").GetString());
        Assert.Single((await ListLotsAsync(owner, ranchId)).EnumerateArray());
    }

    [Fact]
    public async Task LotEndpoints_WithoutAuthentication_AreUnauthorized()
    {
        using var anonymous = factory.CreateClient();
        var ranchId = Guid.NewGuid();

        using var listing = await anonymous.GetAsync(LotsPath(ranchId));
        using var creation = await anonymous.PostJsonAsync(LotsPath(ranchId), new { name = "Unauthorized lot" });

        Assert.Equal(HttpStatusCode.Unauthorized, listing.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, creation.StatusCode);
    }

    [Fact]
    public async Task OpenApi_ExplainsTheGroupingReasonAndSeparatesCreationFromListing()
    {
        using var documentedHost = factory.WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["OpenApi:Enabled"] = "true" })));
        using var client = documentedHost.CreateClient();

        using var response = await client.GetAsync("openapi/v1.json");

        var document = await response.ReadJsonAsync(HttpStatusCode.OK);
        var operations = document.GetProperty("paths").GetProperty("/api/v1/ranches/{ranchId}/lots");
        var post = operations.GetProperty("post");
        Assert.Contains("Creates exactly one empty cattle lot", post.GetProperty("description").GetString());
        Assert.Contains("Animals in lactation", post.GetProperty("description").GetString());
        Assert.True(post.GetProperty("responses").TryGetProperty("201", out _));
        Assert.Contains("one request", operations.GetProperty("get").GetProperty("description").GetString());
        var schemaReference = post.GetProperty("requestBody").GetProperty("content").GetProperty("application/json")
            .GetProperty("schema").GetProperty("$ref").GetString()!;
        var schemaName = schemaReference.Split('/').Last();
        var resourceSchema = document.GetProperty("components").GetProperty("schemas").GetProperty(schemaName);
        var criterionSchema = resourceSchema.GetProperty("properties").GetProperty("criterion");
        Assert.Contains("plain-text reason for grouping", criterionSchema.GetProperty("description").GetString());
        Assert.Contains("Animals in lactation", criterionSchema.GetProperty("description").GetString());
        Assert.DoesNotContain("criterion", resourceSchema.GetProperty("required").EnumerateArray().Select(item => item.GetString()));
    }

    private static string LotsPath(Guid ranchId) => $"api/v1/ranches/{ranchId}/lots";

    private static async Task<JsonElement> ListLotsAsync(HttpClient client, Guid ranchId)
    {
        using var response = await client.GetAsync(LotsPath(ranchId));
        return await response.ReadJsonAsync(HttpStatusCode.OK);
    }
}
