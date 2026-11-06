using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

/// <summary>
/// Provides tracked, ranch-scoped persistence operations; callers enforce access before querying.
/// </summary>
public interface ICattleRepository
{
    /// <summary>
    /// Finds an animal and its stage history within the specified ranch, including inactive animals.
    /// </summary>
    /// <returns>The tracked animal, or null when its identity does not belong to that ranch.</returns>
    Task<Cattle?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the animal occupying an exact trimmed ear tag, including inactive animals that still reserve it.
    /// </summary>
    /// <returns>The tracked animal in the specified ranch, or null when the ear tag is unused.</returns>
    Task<Cattle?> FindByEarTagAsync(Guid ranchId, string earTag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies validated filters within one ranch and orders the requested page by ear tag, then identifier.
    /// </summary>
    /// <returns>A page of tracked animals with stage history, or an empty page when no records match.</returns>
    Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tracks a new animal for the unit of work; this operation does not commit it.
    /// </summary>
    Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default);
}
