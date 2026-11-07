using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

/// <summary>
/// Records an externally determined withdrawal interval for one animal.
/// </summary>
public sealed class WithdrawalPeriod
{
    private WithdrawalPeriod(Guid id, Guid ranchId, Guid cattleId, Guid? interventionId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        Id = id;
        RanchId = ranchId;
        CattleId = cattleId;
        InterventionId = interventionId;
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    /// <summary>
    /// Gets the withdrawal period identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the ranch identifier.
    /// </summary>
    public Guid RanchId { get; }

    /// <summary>
    /// Gets the animal identifier.
    /// </summary>
    public Guid CattleId { get; }

    /// <summary>
    /// Gets the optional clinical intervention identifier.
    /// </summary>
    public Guid? InterventionId { get; }

    /// <summary>
    /// Gets the UTC instant when the period starts.
    /// </summary>
    public DateTimeOffset StartsAt { get; }

    /// <summary>
    /// Gets the UTC instant when the period ends.
    /// </summary>
    public DateTimeOffset EndsAt { get; }

    /// <summary>
    /// Creates a withdrawal period from explicit UTC bounds without calculating a clinical interval.
    /// </summary>
    /// <param name="id">The withdrawal period identifier.</param>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cattleId">The animal identifier.</param>
    /// <param name="interventionId">An optional clinical intervention identifier.</param>
    /// <param name="startsAt">The UTC start instant.</param>
    /// <param name="endsAt">The UTC end instant.</param>
    /// <returns>A valid withdrawal period.</returns>
    /// <exception cref="PlanningDomainException">An identifier, timestamp, or date range is invalid.</exception>
    public static WithdrawalPeriod Create(Guid id, Guid ranchId, Guid cattleId, Guid? interventionId, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (id == Guid.Empty || ranchId == Guid.Empty || cattleId == Guid.Empty || interventionId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "Withdrawal period identifiers must not be empty.");
        }

        if (startsAt.Offset != TimeSpan.Zero || endsAt.Offset != TimeSpan.Zero)
        {
            throw new PlanningDomainException("planning.utc_timestamp_required", "Withdrawal period timestamps must use UTC.");
        }

        if (endsAt < startsAt)
        {
            throw new PlanningDomainException("planning.withdrawal_date_range_invalid", "The withdrawal period end must be on or after its start.");
        }

        return new WithdrawalPeriod(id, ranchId, cattleId, interventionId, startsAt, endsAt);
    }
}
