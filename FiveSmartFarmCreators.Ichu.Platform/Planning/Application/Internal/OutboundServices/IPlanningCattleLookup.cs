namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// Anti-corruption port towards Cattle Information, which answers whether an animal exists in a ranch.
/// </summary>
public interface IPlanningCattleLookup
{
    Task<bool> CattleBelongsToRanchAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);
}
