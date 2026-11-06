using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

/// <summary>
/// Represents a follow-up scheduled for a planned activity.
/// </summary>
public sealed class FollowUp
{
    private FollowUp(Guid id, Guid activityId, DateTimeOffset dueAt)
    {
        Id = id;
        ActivityId = activityId;
        DueAt = dueAt;
    }

    /// <summary>
    /// Gets the follow-up identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the related planned activity identifier.
    /// </summary>
    public Guid ActivityId { get; }

    /// <summary>
    /// Gets the follow-up due instant in UTC.
    /// </summary>
    public DateTimeOffset DueAt { get; }

    /// <summary>
    /// Creates a follow-up after validating its identifiers and UTC timestamp.
    /// </summary>
    /// <param name="id">The follow-up identifier.</param>
    /// <param name="activityId">The related planned activity identifier.</param>
    /// <param name="dueAt">The due instant in UTC.</param>
    /// <returns>A valid follow-up.</returns>
    /// <exception cref="PlanningDomainException">An identifier or timestamp is invalid.</exception>
    public static FollowUp Create(Guid id, Guid activityId, DateTimeOffset dueAt)
    {
        if (id == Guid.Empty || activityId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "Follow-up identifiers must not be empty.");
        }

        if (dueAt.Offset != TimeSpan.Zero)
        {
            throw new PlanningDomainException("planning.utc_timestamp_required", "The follow-up timestamp must use UTC.");
        }

        return new FollowUp(id, activityId, dueAt);
    }
}
