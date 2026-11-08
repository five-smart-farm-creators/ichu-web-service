using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// Schedules work for a ranch or one animal, and owns the applications and follow-ups recorded against it.
/// </summary>
public sealed class PlannedActivity : AggregateRoot
{
    private readonly List<ApplicationRecord> _applications = [];
    private readonly List<FollowUp> _followUps = [];

    // Entity Framework Core materializes activities without running the factory.
    private PlannedActivity()
    {
    }

    public PlannedActivityId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public PlanningCampaignId? CampaignId { get; private set; }

    public Guid? CattleId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTimeOffset ScheduledAt { get; private set; }

    public IReadOnlyCollection<ApplicationRecord> Applications => _applications.AsReadOnly();

    public IReadOnlyCollection<FollowUp> FollowUps => _followUps.AsReadOnly();

    public static Result<PlannedActivity> Create(
        Guid ranchId,
        PlanningCampaignId? campaignId,
        Guid? cattleId,
        string title,
        DateTimeOffset scheduledAt)
    {
        if (ranchId == Guid.Empty || campaignId?.Value == Guid.Empty || cattleId == Guid.Empty)
        {
            return Result<PlannedActivity>.Failure(PlanningErrors.InvalidIdentifier);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<PlannedActivity>.Failure(PlanningErrors.ActivityTitleRequired);
        }

        if (scheduledAt.Offset != TimeSpan.Zero)
        {
            return Result<PlannedActivity>.Failure(PlanningErrors.UtcTimestampRequired);
        }

        return Result<PlannedActivity>.Success(new PlannedActivity
        {
            Id = new PlannedActivityId(Guid.NewGuid()),
            RanchId = ranchId,
            CampaignId = campaignId,
            CattleId = cattleId,
            Title = title.Trim(),
            ScheduledAt = scheduledAt
        });
    }

    /// <summary>
    /// An activity tied to one animal accepts applications only for that animal.
    /// </summary>
    public Result<ApplicationRecord> RecordApplication(Guid cattleId, DateTimeOffset appliedAt, Guid? interventionId)
    {
        if (CattleId is { } plannedCattleId && plannedCattleId != cattleId)
        {
            return Result<ApplicationRecord>.Failure(PlanningErrors.ActivityCattleMismatch);
        }

        var application = ApplicationRecord.Create(Id, cattleId, appliedAt, interventionId);
        if (application.IsFailure)
        {
            return application;
        }

        _applications.Add(application.Value!);
        return application;
    }

    public Result<FollowUp> ScheduleFollowUp(DateTimeOffset dueAt)
    {
        var followUp = FollowUp.Create(Id, dueAt);
        if (followUp.IsFailure)
        {
            return followUp;
        }

        _followUps.Add(followUp.Value!);
        return followUp;
    }
}
