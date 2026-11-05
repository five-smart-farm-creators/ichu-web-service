namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>Anti-corruption port towards Cattle Information.</summary>
public interface ICattleLookup
{
    Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken);

    Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken);
}
