using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// Represents a scheduled Planning activity without duplicating the clinical intervention record.
/// </summary>
public sealed class PlannedActivity
{
    private PlannedActivity(Guid id, Guid ranchId, Guid? campaignId, Guid? cattleId, string title, DateTimeOffset scheduledAt)
    {
        Id = id;
        RanchId = ranchId;
        CampaignId = campaignId;
        CattleId = cattleId;
        Title = title;
        ScheduledAt = scheduledAt;
    }

    /// <summary>
    /// Gets the planned activity identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the ranch identifier.
    /// </summary>
    public Guid RanchId { get; }

    /// <summary>
    /// Gets the optional campaign identifier.
    /// </summary>
    public Guid? CampaignId { get; }

    /// <summary>
    /// Gets the optional animal identifier from CattleInformation.
    /// </summary>
    public Guid? CattleId { get; }

    /// <summary>
    /// Gets the activity title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the scheduled instant in UTC.
    /// </summary>
    public DateTimeOffset ScheduledAt { get; }

    /// <summary>
    /// Creates a scheduled activity after validating its identifiers and timestamp.
    /// </summary>
    /// <param name="id">The activity identifier.</param>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="campaignId">An optional campaign identifier.</param>
    /// <param name="cattleId">An optional animal identifier.</param>
    /// <param name="title">The activity title.</param>
    /// <param name="scheduledAt">The scheduled UTC instant.</param>
    /// <returns>A valid planned activity.</returns>
    /// <exception cref="PlanningDomainException">An identifier, title, or timestamp is invalid.</exception>
    public static PlannedActivity Create(Guid id, Guid ranchId, Guid? campaignId, Guid? cattleId, string title, DateTimeOffset scheduledAt)
    {
        if (id == Guid.Empty || ranchId == Guid.Empty || campaignId == Guid.Empty || cattleId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "Activity identifiers must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new PlanningDomainException("planning.activity_title_required", "An activity title is required.");
        }

        if (scheduledAt.Offset != TimeSpan.Zero)
        {
            throw new PlanningDomainException("planning.utc_timestamp_required", "The scheduled timestamp must use UTC.");
        }

        return new PlannedActivity(id, ranchId, campaignId, cattleId, title.Trim(), scheduledAt);
    }
}
