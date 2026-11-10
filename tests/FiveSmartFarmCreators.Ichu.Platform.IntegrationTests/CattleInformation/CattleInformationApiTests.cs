using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.CattleInformation;

[Collection(ApiCollection.Name)]
public sealed class CattleInformationApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task RegisterCattle_ReturnsCreatedWithOpeningStagePeriod()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0001");

        Assert.Equal("PE-0001", cattle.GetProperty("earTag").GetString());
        Assert.Equal("BrownSwiss", cattle.GetProperty("breed").GetString());
        Assert.Equal("Female", cattle.GetProperty("sex").GetString());
        Assert.Equal("Heifer", cattle.GetProperty("currentStage").GetString());
        Assert.Equal("Active", cattle.GetProperty("status").GetString());
        var period = Assert.Single(cattle.GetProperty("stagePeriods").EnumerateArray());
        Assert.Equal("Heifer", period.GetProperty("lifeStage").GetString());
        Assert.Equal(JsonValueKind.Null, period.GetProperty("validTo").ValueKind);
    }

    [Fact]
    public async Task GetCattle_WithoutCollar_ReturnsProfileWithoutDevice()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var registered = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0002");

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{registered.GetProperty("id").GetGuid()}");

        var profile = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("PE-0002", profile.GetProperty("cattle").GetProperty("earTag").GetString());
        Assert.True(profile.GetProperty("device").GetProperty("isAvailable").GetBoolean());
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("device").GetProperty("collarId").ValueKind);
    }

    [Fact]
    public async Task SearchCattle_ByEarTag_ReturnsOnlyMatchingAnimal()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await Arrange.RegisterCattleAsync(client, ranchId, "PE-0010");
        await Arrange.RegisterCattleAsync(client, ranchId, "PE-0011");

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle?earTag=PE-0011");

        var matches = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("PE-0011", Assert.Single(matches.EnumerateArray()).GetProperty("earTag").GetString());
    }

    [Fact]
    public async Task UpdateCattle_WithCurrentVersion_ChangesDetailsAndStaleVersionConflicts()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0020");
        var cattleId = cattle.GetProperty("id").GetGuid();
        var version = cattle.GetProperty("version").GetGuid();

        using var updated = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}",
            UpdateRequest("PE-0021", breed: 2, version));

        var body = await updated.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("PE-0021", body.GetProperty("earTag").GetString());
        Assert.Equal("Fleckvieh", body.GetProperty("breed").GetString());
        var newVersion = body.GetProperty("version").GetGuid();
        Assert.NotEqual(version, newVersion);

        using var stale = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}",
            UpdateRequest("PE-0022", breed: 2, version));
        var problem = await stale.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("CattleInformation.CattleVersionMismatch", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ChangeLifeStage_ClosesPreviousPeriodAndKeepsHistory()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0030");
        var cattleId = cattle.GetProperty("id").GetGuid();

        using var response = await client.PatchJsonAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/life-stage", new
        {
            lifeStage = 3,
            validFrom = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = cattle.GetProperty("version").GetGuid()
        });

        var body = await response.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("MilkingCow", body.GetProperty("currentStage").GetString());
        var periods = body.GetProperty("stagePeriods").EnumerateArray().ToList();
        Assert.Equal(2, periods.Count);
        Assert.Equal("Heifer", periods[0].GetProperty("lifeStage").GetString());
        Assert.NotEqual(JsonValueKind.Null, periods[0].GetProperty("validTo").ValueKind);
        Assert.Equal("MilkingCow", periods[1].GetProperty("lifeStage").GetString());
    }

    [Fact]
    public async Task CreateLot_AssignCattleAndRemoveIt_KeepsCattleIdentity()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lot = await Arrange.CreateLotAsync(client, ranchId, "Calving pen");
        var lotId = lot.GetProperty("id").GetGuid();
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0040");
        var cattleId = cattle.GetProperty("id").GetGuid();

        var assigned = await Arrange.AssignToLotAsync(client, ranchId, lotId, cattleId, lot.GetProperty("version").GetGuid());

        var membership = Assert.Single(assigned.GetProperty("memberships").EnumerateArray());
        Assert.Equal(cattleId, membership.GetProperty("cattleId").GetGuid());
        using var removal = await client.DeleteAsync(
            $"api/v1/ranches/{ranchId}/lots/{lotId}/cattle/{cattleId}?expectedVersion={assigned.GetProperty("version").GetGuid()}");
        var removed = await removal.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Empty(removed.GetProperty("memberships").EnumerateArray());
        using var stillRegistered = await client.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}");
        await stillRegistered.ReadBodyAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListLots_ReturnsLotsOfTheRanch()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lot = await Arrange.CreateLotAsync(client, ranchId, "Calving pen");

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/lots");

        var lots = await response.ReadJsonAsync(HttpStatusCode.OK);
        var listed = Assert.Single(lots.EnumerateArray());
        Assert.Equal(lot.GetProperty("id").GetGuid(), listed.GetProperty("id").GetGuid());
        Assert.Equal("Calving pen", listed.GetProperty("name").GetString());
    }

    [Fact]
    public async Task DeactivateCattle_RetainsExitReasonAndBlocksFurtherChanges()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattle = await Arrange.RegisterCattleAsync(client, ranchId, "PE-0050");
        var cattleId = cattle.GetProperty("id").GetGuid();

        using var deactivation = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}/deactivations", new
        {
            reason = 2,
            exitDate = DateOnly.FromDateTime(DateTime.UtcNow),
            expectedVersion = cattle.GetProperty("version").GetGuid()
        });

        var inactive = await deactivation.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Inactive", inactive.GetProperty("status").GetString());
        Assert.Equal("Sale", inactive.GetProperty("exitReason").GetString());
        using var update = await client.PutJsonAsync($"api/v1/ranches/{ranchId}/cattle/{cattleId}",
            UpdateRequest("PE-0051", breed: 1, inactive.GetProperty("version").GetGuid()));
        var problem = await update.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("CattleInformation.CattleInactive", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetCattle_ByManagerOfAnotherRanch_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var cattle = await Arrange.RegisterCattleAsync(owner, ranchId, "PE-0060");
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync($"api/v1/ranches/{ranchId}/cattle/{cattle.GetProperty("id").GetGuid()}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("CattleInformation.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    private static object UpdateRequest(string earTag, int breed, Guid expectedVersion) => new
    {
        earTag,
        breed,
        sex = 1,
        birthDate = new DateOnly(2024, 3, 1),
        expectedVersion
    };
}
