using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Planning.Domain;

public class PlannedActivityTests
{
    private static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    private static readonly Guid CattleId = Guid.Parse("33333333-3333-4333-8333-333333333333");
    private static readonly Guid OtherCattleId = Guid.Parse("55555555-5555-4555-8555-555555555555");
    private static readonly DateTimeOffset ScheduledAt = new(2026, 11, 20, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithANonUtcSchedule_IsRejected()
    {
        var localSchedule = new DateTimeOffset(2026, 11, 20, 8, 0, 0, TimeSpan.FromHours(-5));

        var result = PlannedActivity.Create(RanchId, null, CattleId, "Deworming", localSchedule);

        Assert.Equal(PlanningErrors.UtcTimestampRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void Create_WithoutATitle_IsRejected()
    {
        var result = PlannedActivity.Create(RanchId, null, null, " ", ScheduledAt);

        Assert.Equal(PlanningErrors.ActivityTitleRequired.Code, result.DomainError.Code);
    }

    [Fact]
    public void RecordApplication_ForAnotherAnimalThanThePlannedOne_IsRejectedAndRecordsNothing()
    {
        var activity = PlannedActivity.Create(RanchId, null, CattleId, "Deworming", ScheduledAt).Value!;

        var result = activity.RecordApplication(OtherCattleId, ScheduledAt, null);

        Assert.Equal(PlanningErrors.ActivityCattleMismatch.Code, result.DomainError.Code);
        Assert.Empty(activity.Applications);
    }

    [Fact]
    public void RecordApplication_ForThePlannedAnimal_IsKeptByTheActivity()
    {
        var activity = PlannedActivity.Create(RanchId, null, CattleId, "Deworming", ScheduledAt).Value!;

        var result = activity.RecordApplication(CattleId, ScheduledAt.AddHours(1), null);

        Assert.True(result.IsSuccess);
        Assert.Equal(activity.Id, result.Value!.ActivityId);
        Assert.Single(activity.Applications);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(7)]
    public void ScheduleFollowUp_WithANonUtcDueInstant_IsRejectedAndKeepsNoFollowUp(int daysAfterActivity)
    {
        var activity = PlannedActivity.Create(RanchId, null, null, "Weighing", ScheduledAt).Value!;
        var localDue = ScheduledAt.AddDays(daysAfterActivity).ToOffset(TimeSpan.FromHours(-5));

        var result = activity.ScheduleFollowUp(localDue);

        Assert.Equal(PlanningErrors.UtcTimestampRequired.Code, result.DomainError.Code);
        Assert.Empty(activity.FollowUps);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void ScheduleFollowUp_DueAtOrBeforeTheActivity_IsRejectedAndKeepsNoFollowUp(int daysAfterActivity)
    {
        var activity = PlannedActivity.Create(RanchId, null, null, "Weighing", ScheduledAt).Value!;

        var result = activity.ScheduleFollowUp(ScheduledAt.AddDays(daysAfterActivity));

        Assert.True(result.IsFailure);
        Assert.Equal(PlanningErrors.FollowUpDueAtInvalid.Code, result.DomainError.Code);
        Assert.Empty(activity.FollowUps);
    }

    [Fact]
    public void ScheduleFollowUp_DueAfterTheActivity_IsKeptByTheActivity()
    {
        var activity = PlannedActivity.Create(RanchId, null, null, "Weighing", ScheduledAt).Value!;
        var dueAt = ScheduledAt.AddTicks(1);

        var result = activity.ScheduleFollowUp(dueAt);

        Assert.True(result.IsSuccess);
        var followUp = Assert.Single(activity.FollowUps);
        Assert.Same(result.Value, followUp);
        Assert.Equal(activity.Id, followUp.ActivityId);
        Assert.Equal(dueAt, followUp.DueAt);
    }
}
