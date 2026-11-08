using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the withdrawal period request body to its command. The domain validates the interval.
/// </summary>
public static class RecordWithdrawalPeriodCommandFromResourceAssembler
{
    public static RecordWithdrawalPeriodCommand ToCommandFromResource(RecordWithdrawalPeriodResource resource) =>
        new(resource.RanchId, resource.CattleId, resource.InterventionId, resource.StartsAt, resource.EndsAt);
}
