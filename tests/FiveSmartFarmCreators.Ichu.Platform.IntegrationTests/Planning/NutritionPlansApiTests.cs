using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Planning;

[Collection(ApiCollection.Name)]
public sealed class NutritionPlansApiTests(IchuApiFactory factory)
{
    [Fact]
    public async Task RegisterNutritionPlan_ReturnsCreatedPlanWithItsRationItems()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);

        using var response = await PostPlanAsync(client, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 1));

        var plan = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(lotId, plan.GetProperty("lotId").GetGuid());
        Assert.Equal("Dry season ration", plan.GetProperty("name").GetString());
        Assert.Equal("2026-11-01", plan.GetProperty("validFrom").GetString());
        Assert.Equal(JsonValueKind.Null, plan.GetProperty("validTo").ValueKind);
        Assert.Equal(0.8, plan.GetProperty("targetDailyGainKilograms").GetDouble());
        Assert.Equal("Keep water available", plan.GetProperty("notes").GetString());
        var item = Assert.Single(plan.GetProperty("items").EnumerateArray());
        Assert.Equal("Alfalfa hay", item.GetProperty("feed").GetString());
        Assert.Equal("Forage", item.GetProperty("kind").GetString());
        Assert.Equal(6, item.GetProperty("kilogramsPerHeadPerDay").GetDouble());
    }

    [Fact]
    public async Task RationChange_ClosesThePreviousPlanTheDayBeforeTheNewOne()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 1));

        using var change = await PostPlanAsync(client, ranchId, lotId, "Rainy season ration", new DateOnly(2026, 11, 15));

        await change.ReadBodyAsync(HttpStatusCode.Created);
        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans");
        var plans = (await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray().ToList();
        Assert.Equal(new[] { "Rainy season ration", "Dry season ration" }, plans.Select(plan => plan.GetProperty("name").GetString()));
        Assert.Equal("2026-11-14", plans[1].GetProperty("validTo").GetString());
        Assert.Equal(JsonValueKind.Null, plans[0].GetProperty("validTo").ValueKind);
    }

    [Fact]
    public async Task CurrentNutritionPlan_ReturnsThePlanInForceOnTheRequestedDay()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 1));
        await PostPlanAsync(client, ranchId, lotId, "Rainy season ration", new DateOnly(2026, 11, 15));

        using var before = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans/current?on=2026-11-14");
        using var after = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans/current?on=2026-11-15");

        Assert.Equal("Dry season ration", (await before.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("name").GetString());
        Assert.Equal("Rainy season ration", (await after.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("name").GetString());
    }

    [Fact]
    public async Task CurrentNutritionPlan_WithoutAnyPlanCoveringTheDay_IsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 1));

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans/current?on=2026-10-31");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.NoCurrentNutritionPlan", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CurrentNutritionPlan_WithoutDate_UsesTheCurrentUtcDate()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Open ration", new DateOnly(2020, 1, 1));

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans/current");

        Assert.Equal("Open ration", (await response.ReadJsonAsync(HttpStatusCode.OK)).GetProperty("name").GetString());
    }

    [Fact]
    public async Task RationChange_OnTheSameDayAsThePlanInForce_ReturnsConflict()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 15));

        using var response = await PostPlanAsync(client, ranchId, lotId, "Rainy season ration", new DateOnly(2026, 11, 15));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("Planning.NutritionPlanOverlap", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task NutritionPlan_OverlappingABoundedPlan_ReturnsConflictAndSavesNothing()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);
        await PostPlanAsync(client, ranchId, lotId, "Trial ration", new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));

        using var response = await PostPlanAsync(client, ranchId, lotId, "Overlapping ration", new DateOnly(2026, 11, 10), new DateOnly(2026, 11, 20));

        var problem = await response.ReadJsonAsync(HttpStatusCode.Conflict);
        Assert.Equal("Planning.NutritionPlanOverlap", problem.GetProperty("code").GetString());
        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans");
        Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
    }

    [Fact]
    public async Task NutritionPlan_WithoutItems_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var lotId = await LotIdAsync(client, ranchId);

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans", new
        {
            name = "Empty ration",
            validFrom = "2026-11-01",
            items = Array.Empty<object>()
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("Planning.NutritionPlanItemsRequired", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task NutritionPlan_ForUnknownLot_IsNotFound()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await PostPlanAsync(client, ranchId, Guid.NewGuid(), "Dry season ration", new DateOnly(2026, 11, 1));

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.LotNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task NutritionPlan_PostedByVeterinarianWithReadAccess_IsForbidden_ButReadable()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var lotId = await LotIdAsync(owner, ranchId);
        await PostPlanAsync(owner, ranchId, lotId, "Dry season ration", new DateOnly(2026, 11, 1));
        using var veterinarian = factory.CreateClient(TestIdentity.Create());
        await Arrange.RegisterVeterinarianAsync(veterinarian);
        await Arrange.CompleteVeterinarianProfileAsync(veterinarian);
        var advisoryId = await Arrange.RequestHerdAdvisoryAsync(veterinarian, ranchId);
        await Arrange.GrantReadAdvisoryAsync(owner, ranchId, advisoryId, DateTimeOffset.UtcNow.AddDays(1));

        using var write = await PostPlanAsync(veterinarian, ranchId, lotId, "Vet ration", new DateOnly(2026, 12, 1));
        using var read = await veterinarian.GetAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans");

        var problem = await write.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Planning.RanchAccessDenied", problem.GetProperty("code").GetString());
        Assert.Single((await read.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
    }

    private static async Task<Guid> LotIdAsync(HttpClient client, Guid ranchId) =>
        (await Arrange.CreateLotAsync(client, ranchId, "North pasture")).GetProperty("id").GetGuid();

    private static Task<HttpResponseMessage> PostPlanAsync(
        HttpClient client,
        Guid ranchId,
        Guid lotId,
        string name,
        DateOnly validFrom,
        DateOnly? validTo = null) =>
        client.PostJsonAsync($"api/v1/ranches/{ranchId}/lots/{lotId}/nutrition-plans", new
        {
            name,
            validFrom,
            validTo,
            items = new[]
            {
                new { feed = "Alfalfa hay", kind = "Forage", kilogramsPerHeadPerDay = 6 }
            },
            targetDailyGainKilograms = 0.8,
            notes = "Keep water available"
        });
}
