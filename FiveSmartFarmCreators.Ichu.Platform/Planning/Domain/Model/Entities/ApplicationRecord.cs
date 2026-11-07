using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

/// <summary>
/// Records that an application associated with a planned activity took place.
/// </summary>
public sealed class ApplicationRecord
{
    private ApplicationRecord(Guid id, Guid activityId, Guid cattleId, DateTimeOffset appliedAt, Guid? interventionId)
    {
        Id = id;
        ActivityId = activityId;
        CattleId = cattleId;
        AppliedAt = appliedAt;
        InterventionId = interventionId;
    }

    /// <summary>
    /// Gets the application record identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the related planned activity identifier.
    /// </summary>
    public Guid ActivityId { get; }

    /// <summary>
    /// Gets the animal identifier from CattleInformation.
    /// </summary>
    public Guid CattleId { get; }

    /// <summary>
    /// Gets the instant when the application was recorded, in UTC.
    /// </summary>
    public DateTimeOffset AppliedAt { get; }

    /// <summary>
    /// Gets the optional clinical intervention identifier from OperationsMonitoring.
    /// </summary>
    public Guid? InterventionId { get; }

    /// <summary>
    /// Creates an application record without storing dose or clinical history.
    /// </summary>
    /// <param name="id">The application record identifier.</param>
    /// <param name="activityId">The related planned activity identifier.</param>
    /// <param name="cattleId">The animal identifier.</param>
    /// <param name="appliedAt">The UTC application instant.</param>
    /// <param name="interventionId">An optional clinical intervention identifier.</param>
    /// <returns>A valid application record.</returns>
    /// <exception cref="PlanningDomainException">An identifier or timestamp is invalid.</exception>
    public static ApplicationRecord Create(Guid id, Guid activityId, Guid cattleId, DateTimeOffset appliedAt, Guid? interventionId)
    {
        if (id == Guid.Empty || activityId == Guid.Empty || cattleId == Guid.Empty || interventionId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "Application identifiers must not be empty.");
        }

        if (appliedAt.Offset != TimeSpan.Zero)
        {
            throw new PlanningDomainException("planning.utc_timestamp_required", "The application timestamp must use UTC.");
        }

        return new ApplicationRecord(id, activityId, cattleId, appliedAt, interventionId);
    }
}
