using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

/// <summary>
/// Provides tracked lot persistence within a ranch; callers enforce access before querying.
/// </summary>
public interface ILotRepository
{
    /// <summary>
    /// Finds a lot and its stored memberships within the specified ranch.
    /// </summary>
    /// <returns>The tracked lot, or null when its identity does not belong to that ranch.</returns>
    Task<Lot?> FindByIdAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists ranch lots with their stored memberships, including memberships of inactive animals.
    /// </summary>
    /// <returns>Lots ordered by name, then identifier; an empty list when the ranch has no lots.</returns>
    Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracks a new lot for the unit of work; this operation does not commit it.
    /// </summary>
    Task AddAsync(Lot lot, CancellationToken cancellationToken = default);
}
