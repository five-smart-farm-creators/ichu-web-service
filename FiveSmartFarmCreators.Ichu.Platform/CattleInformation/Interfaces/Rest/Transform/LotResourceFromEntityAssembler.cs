using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Transform;

public static class LotResourceFromEntityAssembler
{
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
