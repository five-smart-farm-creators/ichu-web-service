namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
/// Exposes animal facts to other contexts while enforcing the current caller's ranch read access.
/// </summary>
public interface ICattleInformationContextFacade
{
    /// <summary>
    /// Looks up an animal in the authorized ranch without exposing a domain entity.
    /// </summary>
    /// <returns>An identity and lifecycle snapshot, including inactive animals, or null when not found.</returns>
    Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves a lot's active animals across all result pages for authorized cross-context use.
    /// </summary>
    /// <returns>Active animal snapshots, or an empty list when the lot is missing or has no active members.</returns>
    Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);
}
