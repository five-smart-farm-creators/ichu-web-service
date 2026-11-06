namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;

/// <summary>
///     Anti-corruption port towards Cattle Information.
/// </summary>
public interface ICattleLookup
{
    /// <summary>
    ///     Finds the monitoring-relevant data of an animal asynchronously.
    /// </summary>
    /// <param name="cattleId">The animal identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the animal data, or <see langword="null"/> when the animal does not exist.</returns>
    Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken);

    /// <summary>
    ///     Determines whether a lot belongs to a ranch asynchronously.
    /// </summary>
    /// <param name="lotId">The lot identifier.</param>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing <see langword="true"/> when the lot belongs to the ranch.</returns>
    Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken);
}
