using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects a withdrawal period to the REST contract with its explicit UTC bounds.
/// </summary>
public static class WithdrawalPeriodResourceFromEntityAssembler
{
    public static WithdrawalPeriodResource ToResourceFromEntity(WithdrawalPeriod period) =>
        new(period.Id.Value, period.RanchId, period.CattleId, period.InterventionId, period.StartsAt, period.EndsAt);
}
