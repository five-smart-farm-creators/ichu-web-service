namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Anti-corruption port towards Cattle Information.
/// </summary>
public interface ICattleLookup
{
    /// <summary>
    ///     Finds the monitoring-relevant data of an animal, or null when the animal does not exist.
    /// </summary>
    Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken);

    Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken);
}
