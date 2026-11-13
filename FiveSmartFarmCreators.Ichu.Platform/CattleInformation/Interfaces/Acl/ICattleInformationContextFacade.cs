namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
///     Exposes animal and lot facts to other contexts. Ranch-scoped methods hide resources callers cannot read;
///     trusted in-process callers may resolve ownership and must authorize the result themselves.
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
    ///     Resolves the ranch that owns a lot for a trusted in-process caller that authorizes the result itself.
    /// </summary>
    Task<Guid?> FindRanchIdForLotAsync(Guid lotId, CancellationToken cancellationToken);

    /// <summary>
    ///     Resolves a lot's active animals across all result pages.
    /// </summary>
    /// <returns>Active animal snapshots, or an empty list when the lot is unknown or not visible to the caller.</returns>
    Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns the birth and exit dates of an animal in any ranch, for trusted in-process callers that authorize the result themselves.
    /// </summary>
    Task<CattleLifecycleSnapshot?> FindLifecycleAsync(Guid cattleId, CancellationToken cancellationToken);

    /// <summary>
    ///     Lists every animal of the ranch, active and exited.
    /// </summary>
    /// <returns>Herd member snapshots, or an empty list when the ranch is not visible to the caller.</returns>
    Task<IReadOnlyList<HerdMemberSnapshot>> GetHerdAsync(Guid ranchId, CancellationToken cancellationToken);
}
