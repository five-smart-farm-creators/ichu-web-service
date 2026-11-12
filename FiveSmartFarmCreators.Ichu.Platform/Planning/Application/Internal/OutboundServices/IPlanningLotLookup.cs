namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Anti-corruption port towards Cattle Information, which answers whether a lot exists in a ranch.
/// </summary>
public interface IPlanningLotLookup
{
    Task<bool> LotBelongsToRanchAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);
}
