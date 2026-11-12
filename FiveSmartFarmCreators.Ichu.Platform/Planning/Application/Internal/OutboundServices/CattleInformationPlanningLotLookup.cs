using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Resolves lots through the Cattle Information facade, which hides the lots of other ranches as unknown.
/// </summary>
public sealed class CattleInformationPlanningLotLookup(ICattleInformationContextFacade cattleInformation) : IPlanningLotLookup
{
    /// <inheritdoc />
    public Task<bool> LotBelongsToRanchAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
        cattleInformation.LotExistsAsync(ranchId, lotId, cancellationToken);
}
