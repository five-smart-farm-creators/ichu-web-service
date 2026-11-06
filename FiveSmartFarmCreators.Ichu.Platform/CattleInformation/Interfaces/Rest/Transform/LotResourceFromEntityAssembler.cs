using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

/// <summary>
/// Projects stored lot memberships without treating membership alone as proof of active inventory.
/// </summary>
public static class LotResourceFromEntityAssembler
{
    /// <summary>
    /// Orders stored memberships by assignment time and animal identifier without filtering inactive animals.
    /// </summary>
    public static LotResource ToResourceFromEntity(Lot lot)
    {
        var memberships = lot.Memberships
            .OrderBy(membership => membership.AssignedAt)
            .ThenBy(membership => membership.CattleId)
            .Select(membership => new LotMembershipResource(membership.CattleId, membership.AssignedAt))
            .ToList();
        return new LotResource(lot.Id, lot.RanchId, lot.Name, lot.Criterion, lot.Version, memberships);
    }
}
