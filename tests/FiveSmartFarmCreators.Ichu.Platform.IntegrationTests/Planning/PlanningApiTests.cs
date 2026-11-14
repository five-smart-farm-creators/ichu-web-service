using System.Net;
using System.Text.Json;
using FiveSmartFarmCreators.Ichu.Platform.IntegrationTests.Harness;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        using var single = await client.GetAsync($"api/v1/planning-campaigns/{campaignId}");

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
        using var response = await client.GetAsync($"api/v1/planned-activities/{activity.GetProperty("id").GetGuid()}");
        await response.ReadBodyAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ScheduleActivity_WithoutScheduledAt_ReturnsBadRequestAndRecordsNothing()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities", new
        {
            title = "Missing schedule"
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.True(problem.TryGetProperty("errors", out _));
        await AssertNoActivitiesAsync(ranchId);
    }

    [Fact]
    public async Task ScheduleActivity_WithANonUtcSchedule_ReturnsBadRequestAndRecordsNothing()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();

        using var response = await client.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities", new
        {
            title = "Local schedule",
            scheduledAt = ScheduledAt.ToOffset(TimeSpan.FromHours(-5))
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("Planning.UtcTimestampRequired", problem.GetProperty("code").GetString());
        await AssertNoActivitiesAsync(ranchId);
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

        using var response = await client.PostJsonAsync($"api/v1/planned-activities/{activityId}/applications", new
        {
            cattleId,
            appliedAt,
            interventionId
        });

        var application = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(cattleId, application.GetProperty("cattleId").GetGuid());
        Assert.Equal(interventionId, application.GetProperty("interventionId").GetGuid());
        using var listing = await client.GetAsync($"api/v1/planned-activities/{activityId}/applications");
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

        using var response = await client.PostJsonAsync($"api/v1/planned-activities/{activityId}/follow-ups", new { dueAt });

        var created = await response.ReadJsonAsync(HttpStatusCode.Created);
        Assert.Equal(activityId, created.GetProperty("activityId").GetGuid());
        Assert.Equal(dueAt, created.GetProperty("dueAt").GetDateTimeOffset());
        using var listing = await client.GetAsync($"api/v1/planned-activities/{activityId}/follow-ups");
        var followUp = Assert.Single((await listing.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
        Assert.Equal(created.GetProperty("id").GetGuid(), followUp.GetProperty("id").GetGuid());
        Assert.Equal(activityId, followUp.GetProperty("activityId").GetGuid());
        Assert.Equal(dueAt, followUp.GetProperty("dueAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task ScheduleFollowUp_WithoutDueAt_ReturnsBadRequestAndRecordsNothing()
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(client, ranchId, null, null, "Check vaccination response")).GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/planned-activities/{activityId}/follow-ups", new { });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.True(problem.TryGetProperty("errors", out _));
        await AssertNoFollowUpsAsync(client, activityId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task ScheduleFollowUp_DueAtOrBeforeTheActivity_ReturnsBadRequestAndRecordsNothing(int daysAfterActivity)
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(client, ranchId, null, null, "Check vaccination response")).GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/planned-activities/{activityId}/follow-ups", new
        {
            dueAt = ScheduledAt.AddDays(daysAfterActivity)
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("Planning.FollowUpDueAtInvalid", problem.GetProperty("code").GetString());
        Assert.Equal("The follow-up due time must be after the activity's scheduled time.", problem.GetProperty("title").GetString());
        await AssertNoFollowUpsAsync(client, activityId);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(7)]
    public async Task ScheduleFollowUp_WithANonUtcDueInstant_ReturnsBadRequestAndRecordsNothing(int daysAfterActivity)
    {
        using var client = factory.CreateClient(TestIdentity.Create());
        var ranchId = await client.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(client, ranchId, null, null, "Check vaccination response")).GetProperty("id").GetGuid();

        using var response = await client.PostJsonAsync($"api/v1/planned-activities/{activityId}/follow-ups", new
        {
            dueAt = ScheduledAt.AddDays(daysAfterActivity).ToOffset(TimeSpan.FromHours(-5))
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.BadRequest);
        Assert.Equal("Planning.UtcTimestampRequired", problem.GetProperty("code").GetString());
        await AssertNoFollowUpsAsync(client, activityId);
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
        await AssertNoActivitiesAsync(plannerRanchId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/follow-ups")]
    public async Task GetActivityOrFollowUps_ByManagerWithoutRanchAccess_IsNotFound(string suffix)
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(owner, ranchId, null, null, "Private activity")).GetProperty("id").GetGuid();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync($"api/v1/planned-activities/{activityId}{suffix}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.ActivityNotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ScheduleFollowUp_ByManagerWithoutRanchAccess_IsNotFoundAndRecordsNothing()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        var activityId = (await ScheduleActivityAsync(owner, ranchId, null, null, "Private activity")).GetProperty("id").GetGuid();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PostJsonAsync($"api/v1/planned-activities/{activityId}/follow-ups", new
        {
            dueAt = ScheduledAt.AddDays(1)
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.ActivityNotFound", problem.GetProperty("code").GetString());
        await AssertNoFollowUpsAsync(owner, activityId);
    }

    [Fact]
    public async Task ScheduleActivity_ByManagerWithoutRanchAccess_IsForbiddenAndRecordsNothing()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.PostJsonAsync($"api/v1/ranches/{ranchId}/planned-activities", new
        {
            title = "Foreign activity",
            scheduledAt = ScheduledAt
        });

        var problem = await response.ReadJsonAsync(HttpStatusCode.Forbidden);
        Assert.Equal("Planning.RanchAccessDenied", problem.GetProperty("code").GetString());
        await AssertNoActivitiesAsync(ranchId);
    }

    [Fact]
    public async Task Calendar_ByManagerWithoutRanchAccess_IsForbidden()
    {
        using var owner = factory.CreateClient(TestIdentity.Create());
        var ranchId = await owner.RegisterManagerAsync();
        await ScheduleActivityAsync(owner, ranchId, null, null, "Private activity");
        using var outsider = factory.CreateClient(TestIdentity.Create());
        await outsider.RegisterManagerAsync();

        using var response = await outsider.GetAsync(
            $"api/v1/ranches/{ranchId}/calendar?from={ScheduledAt.ToQueryValue()}&to={ScheduledAt.AddDays(1).ToQueryValue()}");

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
        await other.RegisterManagerAsync();

        using var response = await other.GetAsync($"api/v1/planning-campaigns/{campaignId}");

        var problem = await response.ReadJsonAsync(HttpStatusCode.NotFound);
        Assert.Equal("Planning.CampaignNotFound", problem.GetProperty("code").GetString());
    }

    private async Task AssertNoActivitiesAsync(Guid ranchId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.False(await context.Set<PlannedActivity>().AnyAsync(activity => activity.RanchId == ranchId));
    }

    private static async Task AssertNoFollowUpsAsync(HttpClient client, Guid activityId)
    {
        using var response = await client.GetAsync($"api/v1/planned-activities/{activityId}/follow-ups");
        Assert.Empty((await response.ReadJsonAsync(HttpStatusCode.OK)).EnumerateArray());
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
