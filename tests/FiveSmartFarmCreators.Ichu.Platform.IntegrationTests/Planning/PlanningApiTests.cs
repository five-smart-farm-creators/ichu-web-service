using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;

namespace FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Planning;

[Collection(ApiCollection.Name)]
public sealed class PlanningApiTests(IchuApiFactory factory)
{
    private static readonly DateTimeOffset ScheduledAt = new(2026, 11, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateCampaign_ReturnsCreatedCampaign()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        var campaign = await CreateCampaignAsync(client, ranchId, "Brucellosis vaccination");

        Assert.Equal(ranchId, campaign.GetProperty("ranchId").GetGuid());
        Assert.Equal("Brucellosis vaccination", campaign.GetProperty("name").GetString());
        Assert.Equal("Sanitary", campaign.GetProperty("kind").GetString());
        Assert.Equal("2026-11-01", campaign.GetProperty("startsOn").GetString());
        Assert.Equal("2026-11-30", campaign.GetProperty("endsOn").GetString());
    }

    [Fact]
    public async Task CreateCampaign_EndingBeforeStart_ReturnsValidationProblem()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planning-campaigns", new
        {
            name = "Invalid campaign",
            kind = 0,
            startsOn = "2026-11-30",
            endsOn = "2026-11-01"
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("Planning.CampaignDateRangeInvalid", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetCampaigns_ListsCampaignsOfTheRanch()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var campaign = await CreateCampaignAsync(client, ranchId, "Brucellosis vaccination");
        var campaignId = campaign.GetProperty("id").GetGuid();

        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/planning-campaigns");
        using var single = await client.GetAsync($"api/v1/ranches/{ranchId}/planning-campaigns/{campaignId}");

        var listed = Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(campaignId, listed.GetProperty("id").GetGuid());
        var found = await single.ReadJsonAsync(HttpStatusCode.OK);
        Assert.Equal("Brucellosis vaccination", found.GetProperty("name").GetString());
    }

    [Fact]
    public async Task ScheduleActivity_ForCampaignAndCattle_ReturnsCreatedActivity()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var campaignId = (await CreateCampaignAsync(client, ranchId, "Brucellosis vaccination")).GetProperty("id").GetGuid();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0400")).GetProperty("id").GetGuid();

        var activity = await ScheduleActivityAsync(client, ranchId, campaignId, cattleId, "Vaccinate heifer");

        Assert.Equal(ranchId, activity.GetProperty("ranchId").GetGuid());
        Assert.Equal(campaignId, activity.GetProperty("campaignId").GetGuid());
        Assert.Equal(cattleId, activity.GetProperty("cattleId").GetGuid());
        Assert.Equal("Vaccinate heifer", activity.GetProperty("title").GetString());
        Assert.Equal(ScheduledAt, activity.GetProperty("scheduledAt").GetDateTimeOffset());
        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/planned-activities/{activity.GetProperty("id").GetGuid()}");
        await response.ReadBodyAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RecordApplication_LinkedToClinicalIntervention_IsListedWithActivity()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0401", lifeStage: 3)).GetProperty("id").GetGuid();
        var activityId = (await ScheduleActivityAsync(client, ranchId, null, cattleId, "Vaccinate cow")).GetProperty("id").GetGuid();
        var interventionId = await RegisterVaccinationAsync(client, ranchId, cattleId);
        var appliedAt = ScheduledAt.AddHours(1);

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities/{activityId}/applications", new
        {
            cattleId,
            appliedAt,
            interventionId
        });

        var application = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(cattleId, application.GetProperty("cattleId").GetGuid());
        Assert.Equal(interventionId, application.GetProperty("interventionId").GetGuid());
        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/planned-activities/{activityId}/applications");
        var applications = Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(appliedAt, applications.GetProperty("appliedAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task ScheduleFollowUp_IsListedWithItsDueInstant()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(client, ranchId, null, null, "Check vaccination response")).GetProperty("id").GetGuid();
        var dueAt = ScheduledAt.AddDays(14);

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities/{activityId}/follow-ups", new { dueAt });

        await response.ReadBodyAsync(HttpStatusCode.Created);
        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/planned-activities/{activityId}/follow-ups");
        var followUp = Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(dueAt, followUp.GetProperty("dueAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task Calendar_ReturnsOnlyActivitiesInsideTheInterval()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        await ScheduleActivityAsync(client, ranchId, null, null, "Inside the interval");
        using var outsideResponse = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities", new
        {
            title = "Outside the interval",
            scheduledAt = new DateTimeOffset(2027, 1, 10, 9, 0, 0, TimeSpan.Zero)
        });
        await outsideResponse.ReadBodyAsync(HttpStatusCode.Created);

        using var response = await client.GetAsync(
            $"api/v1/ranches/{ranchId}/calendar?from={new DateTimeOffset(2026, 11, 1, 0, 0, 0, TimeSpan.Zero).ToQueryValue()}" +
            $"&to={new DateTimeOffset(2026, 11, 30, 23, 59, 59, TimeSpan.Zero).ToQueryValue()}");

        var calendar = Assert.Single((await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal("Inside the interval", calendar.GetProperty("title").GetString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("?from=2026-11-01T00:00:00Z")]
    [InlineData("?to=2026-11-30T23:59:59Z")]
    public async Task Calendar_WithoutBothBounds_ReturnsBadRequest(string query)
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.GetAsync($"api/v1/ranches/{ranchId}/calendar{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithdrawalPeriod_RecordedForAnimal_IsListedForRanch()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var cattleId = (await Arrange.RegisterCattleAsync(client, ranchId, "PE-0402", lifeStage: 3)).GetProperty("id").GetGuid();
        var interventionId = await RegisterVaccinationAsync(client, ranchId, cattleId);
        var startsAt = ScheduledAt;
        var endsAt = ScheduledAt.AddDays(21);

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/withdrawal-periods", new
        {
            cattleId,
            interventionId,
            startsAt,
            endsAt
        });

        var period = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(interventionId, period.GetProperty("interventionId").GetGuid());
        using var listing = await client.GetAsync($"api/v1/ranches/{ranchId}/withdrawal-periods?cattleId={cattleId}");
        var listed = Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(startsAt, listed.GetProperty("startsAt").GetDateTimeOffset());
        Assert.Equal(endsAt, listed.GetProperty("endsAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task ScheduleActivity_ForCattleOfAnotherRanch_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var planner = factory.CreateClient(TestIdentity.Create());
        var plannerRanchId = await planner.RegisterManagerAsync();
        var foreignCattleId = (await Arrange.RegisterCattleAsync(owner, ranchId, "PE-0403")).GetProperty("id").GetGuid();

        using var response = await planner.PostJsonAsync($"api/v1/ranches/{plannerRanchId}/planned-activities", new
        {
            cattleId = foreignCattleId,
            title = "Foreign animal",
            scheduledAt = ScheduledAt
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.CattleNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task GetActivity_ByManagerOfAnotherRanch_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(owner, ranchId, null, null, "Private activity")).GetProperty("id").GetGuid();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync($"api/v1/ranches/{ranchId}/planned-activities/{activityId}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Planning.RanchAccessDenied", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task CreateCampaign_WithKindName_ReturnsTheKindName()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planning-campaigns", new
        {
            name = "Reproductive review",
            kind = "Reproductive",
            startsOn = "2026-11-01",
            endsOn = "2026-11-30"
        });

        var campaign = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal("Reproductive", campaign.GetProperty("kind").GetString());
    }

    [Fact]
    public async Task GetCampaign_UnderAnotherRanchOfTheCaller_IsNotFound()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var campaignId = (await CreateCampaignAsync(owner, ranchId, "Brucellosis vaccination")).GetProperty("id").GetGuid();
        using var other = factory.CreateClient(TestIdentity.Create());
        var otherRanchId = await other.RegisterManagerAsync();

        using var response = await other.GetAsync($"api/v1/ranches/{otherRanchId}/planning-campaigns/{campaignId}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.CampaignNotFound", problem.GetProperty("code").GetString());
    }

    private static async Task<JsonElement> CreateCampaignAsync(HttpClient client, Guid ranchId, string name)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planning-campaigns", new
        {
            name,
            kind = 0,
            startsOn = "2026-11-01",
            endsOn = "2026-11-30"
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    private static async Task<JsonElement> ScheduleActivityAsync(HttpClient client, Guid ranchId, Guid? campaignId, Guid? cattleId, string title)
    {
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities", new
        {
            campaignId,
            cattleId,
            title,
            scheduledAt = ScheduledAt
        });
        return await response.ReadJsonAsync(HttpStatusCode.Created);
    }

    private static async Task<Guid> RegisterVaccinationAsync(HttpClient client, Guid ranchId, Guid cattleId)
    {
        var operationId = Guid.NewGuid();
        // Clinical records cannot describe the future, unlike the planned schedule.
        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/clinical-interventions", new
        {
            operationId,
            cattleId,
            type = "Vaccination",
            occurredAt = DateTimeOffset.UtcNow.AddHours(-2),
            product = "Brucellosis vaccine",
            dose = "2 ml"
        });
        await response.ReadBodyAsync(HttpStatusCode.Created);
        return operationId;
    }
}
