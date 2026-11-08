using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;

/// <summary>
/// Records an externally determined withdrawal interval for one animal. Planning stores the bounds it receives and never calculates them.
/// </summary>
public sealed class WithdrawalPeriod : AggregateRoot
{
    // Entity Framework Core materializes withdrawal periods without running the factory.
    private WithdrawalPeriod()
    {
    }

    public WithdrawalPeriodId Id { get; private set; }

    public Guid RanchId { get; private set; }

    public Guid CattleId { get; private set; }

    public Guid? InterventionId { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public static Result<WithdrawalPeriod> Create(
        Guid ranchId,
        Guid cattleId,
        Guid? interventionId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt)
    {
        if (ranchId == Guid.Empty || cattleId == Guid.Empty || interventionId == Guid.Empty)
        {
            return Result<WithdrawalPeriod>.Failure(PlanningErrors.InvalidIdentifier);
        }

        if (startsAt.Offset != TimeSpan.Zero || endsAt.Offset != TimeSpan.Zero)
        {
            return Result<WithdrawalPeriod>.Failure(PlanningErrors.UtcTimestampRequired);
        }

        if (endsAt < startsAt)
        {
            return Result<WithdrawalPeriod>.Failure(PlanningErrors.WithdrawalDateRangeInvalid);
        }

        return Result<WithdrawalPeriod>.Success(new WithdrawalPeriod
        {
            Id = new WithdrawalPeriodId(Guid.NewGuid()),
            RanchId = ranchId,
            CattleId = cattleId,
            InterventionId = interventionId,
            StartsAt = startsAt,
            EndsAt = endsAt
        });
    }
}
