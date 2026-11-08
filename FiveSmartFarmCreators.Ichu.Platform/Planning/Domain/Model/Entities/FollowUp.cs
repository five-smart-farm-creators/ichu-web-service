using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

/// <summary>
/// Represents a follow-up that falls due after a planned activity.
/// </summary>
public sealed class FollowUp
{
    // Entity Framework Core materializes follow-ups without running the constructor.
    private FollowUp()
    {
    }

    public FollowUpId Id { get; private set; }

    public PlannedActivityId ActivityId { get; private set; }

    public DateTimeOffset DueAt { get; private set; }

    internal static Result<FollowUp> Create(PlannedActivityId activityId, DateTimeOffset dueAt)
    {
        if (dueAt.Offset != TimeSpan.Zero)
        {
            return Result<FollowUp>.Failure(PlanningErrors.UtcTimestampRequired);
        }

        return Result<FollowUp>.Success(new FollowUp
        {
            Id = new FollowUpId(Guid.NewGuid()),
            ActivityId = activityId,
            DueAt = dueAt
        });
    }
}
