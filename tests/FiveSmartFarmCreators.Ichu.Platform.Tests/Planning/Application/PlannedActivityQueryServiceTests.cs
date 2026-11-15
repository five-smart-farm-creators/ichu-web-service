using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Application;

public class PlannedActivityQueryServiceTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid OtherRanchId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    private static readonly Guid AccountId = Guid.Parse("44444444-4444-4444-8444-444444444444");
    private static readonly DateTimeOffset From = new(2026, 11, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly FakePlanningAccess _access = new() { AccountId = AccountId };
    private readonly FakeActivities _activities = new();
    private readonly PlannedActivityQueryService _service;

    public PlannedActivityQueryServiceTests()
    {
        var guard = new PlanningGuard(_access, new FakeCattleLookup(), new FakeInterventionLookup(), new FakeLotLookup());
        _service = new PlannedActivityQueryService(_activities, guard);
    }

    [Fact]
    public async Task Calendar_EndingBeforeItStarts_IsRejected()
    {
        var result = await _service.HandleAsync(
            new GetPlanningCalendarQuery(RanchId, From.AddDays(30), From), CancellationToken.None);

        Assert.Equal(PlanningErrors.CalendarRangeInvalid.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task Calendar_WithoutReadAccess_IsForbidden()
    {
        _access.Grants = [];

        var result = await _service.HandleAsync(
            new GetPlanningCalendarQuery(RanchId, From, From.AddDays(30)), CancellationToken.None);

        Assert.Equal(PlanningErrors.RanchAccessDenied.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task GetActivity_OfAnUnknownIdentifier_IsNotFound()
    {
        var result = await _service.HandleAsync(
            new GetPlannedActivityByIdQuery(new PlannedActivityId(Guid.NewGuid())), CancellationToken.None);

        Assert.Equal(PlanningErrors.ActivityNotFound.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task GetActivity_OfAnotherRanch_IsNotFound()
    {
        var activity = PlannedActivity.Create(OtherRanchId, null, null, "Vaccination", From).Value!;
        _activities.Items.Add(activity);
        _access.DeniedRanches.Add(OtherRanchId);

        var result = await _service.HandleAsync(new GetPlannedActivityByIdQuery(activity.Id), CancellationToken.None);

        Assert.Equal(PlanningErrors.ActivityNotFound.Code, result.DomainError.Code);
    }

    [Fact]
    public async Task GetApplications_ListsTheRecordsInApplicationOrder()
    {
        var activity = PlannedActivity.Create(RanchId, null, null, "Vaccination", From).Value!;
        var animal = Guid.NewGuid();
        activity.RecordApplication(animal, From.AddDays(2), null);
        activity.RecordApplication(animal, From.AddDays(1), null);
        _activities.Items.Add(activity);

        var result = await _service.HandleAsync(new GetApplicationRecordsByActivityQuery(activity.Id), CancellationToken.None);

        Assert.Equal(new[] { From.AddDays(1), From.AddDays(2) }, result.Value!.Select(application => application.AppliedAt));
    }
}
