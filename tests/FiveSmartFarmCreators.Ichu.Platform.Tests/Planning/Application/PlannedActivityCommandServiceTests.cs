using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class PlannedActivityCommandServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid OtherCattleId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    private static readonly DateTimeOffset ScheduledAt = new(2026, 11, 20, 8, 0, 0, TimeSpan.Zero);

    private readonly FakePlanningAccess _access = new() { AccountId = AccountId };
    private readonly FakeCampaigns _campaigns = new();
    private readonly FakeActivities _activities = new();
    private readonly FakeCattleLookup _cattle = new();
    private readonly FakeInterventionLookup _interventions = new();
    private readonly CountingUnitOfWork _unitOfWork = new();
    private readonly PlannedActivityCommandService _service;

    public PlannedActivityCommandServiceTests()
    {
        var guard = new PlanningGuard(_access, _cattle, _interventions);
        _service = new PlannedActivityCommandService(_activities, _campaigns, _unitOfWork, guard);
    }

    [Fact]
    public async Task Schedule_AddsTheActivityAndCompletesTheUnitOfWork()
    {
        var result = await _service.HandleAsync(Schedule(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(_activities.Items);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Schedule_WithoutAVerifiedAccount_IsForbiddenAndSavesNothing()
    {
        _access.AccountId = null;

        var result = await _service.HandleAsync(Schedule(), CancellationToken.None);

        Assert.Equal(PlanningErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Equal(EErrorType.Forbidden, result.DomainError.Type);
        Assert.Empty(_activities.Items);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task Schedule_WithACampaignOfAnotherRanch_IsAConflict()
    {
        var campaign = PlanningCampaign.Create(
            OtherRanchId, "Autumn campaign", CampaignKind.Sanitary, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30)).Value!;
        _campaigns.Items.Add(campaign);

        var result = await _service.HandleAsync(Schedule(campaignId: campaign.Id), CancellationToken.None);

        Assert.Equal(PlanningErrors.CampaignRanchMismatch.Code, result.DomainError.Code);
        Assert.Equal(EErrorType.Conflict, result.DomainError.Type);
        Assert.Empty(_activities.Items);
    }

    [Fact]
    public async Task Schedule_WithAnAnimalOfAnotherRanch_IsNotFound()
    {
        _cattle.Members.Add((OtherRanchId, CattleId));

        var result = await _service.HandleAsync(Schedule(cattleId: CattleId), CancellationToken.None);

        Assert.Equal(PlanningErrors.CattleNotFound.Code, result.DomainError.Code);
        Assert.Equal(EErrorType.NotFound, result.DomainError.Type);
        Assert.Empty(_activities.Items);
    }

    [Fact]
    public async Task RecordApplication_ForAnotherAnimalOfThePlan_IsAConflictAndSavesNothing()
    {
        var activity = AddActivity(cattleId: CattleId);
        _cattle.Members.Add((RanchId, OtherCattleId));

        var result = await _service.HandleAsync(
            new RecordApplicationCommand(activity.Id, OtherCattleId, ScheduledAt, null), CancellationToken.None);

        Assert.Equal(PlanningErrors.ActivityCattleMismatch.Code, result.DomainError.Code);
        Assert.Empty(activity.Applications);
        Assert.Equal(0, _unitOfWork.CompletedCount);
    }

    [Fact]
    public async Task RecordApplication_WithoutClinicalPermission_IsForbidden()
    {
        var activity = AddActivity(cattleId: CattleId);
        _cattle.Members.Add((RanchId, CattleId));
        _access.Grants = [PlanningPermission.ReadPlanning, PlanningPermission.ManagePlanning];

        var result = await _service.HandleAsync(
            new RecordApplicationCommand(activity.Id, CattleId, ScheduledAt, null), CancellationToken.None);

        Assert.Equal(PlanningErrors.RanchAccessDenied.Code, result.DomainError.Code);
        Assert.Empty(activity.Applications);
    }

    [Fact]
    public async Task RecordApplication_LinkedToAnInterventionOfAnotherAnimal_IsAConflict()
    {
        var activity = AddActivity();
        var interventionId = Guid.NewGuid();
        _cattle.Members.Add((RanchId, CattleId));
        _interventions.Interventions[interventionId] = new ClinicalInterventionSnapshot(interventionId, RanchId, OtherCattleId);

        var result = await _service.HandleAsync(
            new RecordApplicationCommand(activity.Id, CattleId, ScheduledAt, interventionId), CancellationToken.None);

        Assert.Equal(PlanningErrors.InterventionCattleMismatch.Code, result.DomainError.Code);
        Assert.Empty(activity.Applications);
    }

    [Fact]
    public async Task RecordApplication_ForThePlannedAnimal_IsSavedWithTheActivity()
    {
        var activity = AddActivity(cattleId: CattleId);
        _cattle.Members.Add((RanchId, CattleId));

        var result = await _service.HandleAsync(
            new RecordApplicationCommand(activity.Id, CattleId, ScheduledAt, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(activity.Applications);
        Assert.Equal(1, _unitOfWork.CompletedCount);
    }

    private static SchedulePlanningActivityCommand Schedule(PlanningCampaignId? campaignId = null, Guid? cattleId = null) =>
        new(RanchId, campaignId, cattleId, "Vaccination", ScheduledAt);

    private PlannedActivity AddActivity(Guid? cattleId = null)
    {
        var activity = PlannedActivity.Create(RanchId, null, cattleId, "Deworming", ScheduledAt).Value!;
        _activities.Items.Add(activity);
        return activity;
    }
}
