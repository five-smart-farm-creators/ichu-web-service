using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Resolves a lot's owning ranch through the Cattle Information facade.
/// </summary>
public sealed class CattleInformationPlanningLotLookup(ICattleInformationContextFacade cattleInformation) : IPlanningLotLookup
{
    /// <inheritdoc />
    public Task<Guid?> FindRanchIdAsync(Guid lotId, CancellationToken cancellationToken) =>
        cattleInformation.FindRanchIdForLotAsync(lotId, cancellationToken);
}
