namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
///     Exposes animal and lot facts to other contexts. Callers that lack ranch read access receive the same answer as
///     for an unknown identifier.
/// </summary>
public interface ICattleInformationContextFacade
{
    /// <summary>
    ///     Looks up an animal in the ranch, including inactive animals.
    /// </summary>
    /// <returns>An identity and lifecycle snapshot, or null when the animal is unknown or not visible to the caller.</returns>
    Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);

    /// <summary>
    ///     Reports whether a lot exists in the ranch, including an empty lot.
    /// </summary>
    Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);

    /// <summary>
    ///     Resolves a lot's active animals across all result pages.
    /// </summary>
    /// <returns>Active animal snapshots, or an empty list when the lot is unknown or not visible to the caller.</returns>
    Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the birth and exit dates of an animal in any ranch, for trusted in-process callers that authorize the result themselves.
    /// </summary>
    Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken);
}
