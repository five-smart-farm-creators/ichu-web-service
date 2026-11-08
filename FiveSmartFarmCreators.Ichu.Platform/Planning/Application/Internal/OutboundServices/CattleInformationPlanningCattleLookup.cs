using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Resolves animals through the Cattle Information facade, which hides animals of other ranches as unknown.
/// </summary>
public sealed class CattleInformationPlanningCattleLookup(ICattleInformationContextFacade cattleInformation) : IPlanningCattleLookup
{
    /// <inheritdoc />
    public async Task<bool> CattleBelongsToRanchAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken) =>
        await cattleInformation.FindByIdAsync(ranchId, cattleId, cancellationToken) is not null;
}
