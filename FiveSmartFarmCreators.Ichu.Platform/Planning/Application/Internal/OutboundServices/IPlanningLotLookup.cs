namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Anti-corruption port towards Cattle Information, which resolves the owning ranch for a lot.
/// </summary>
public interface IPlanningLotLookup
{
    Task<Guid?> FindRanchIdAsync(Guid lotId, CancellationToken cancellationToken);
}
