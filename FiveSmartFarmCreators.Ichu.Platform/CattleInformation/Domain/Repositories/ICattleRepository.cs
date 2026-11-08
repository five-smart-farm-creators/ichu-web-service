using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

/// <summary>
///     Provides ranch-scoped animal persistence. Callers check access before querying.
/// </summary>
public interface ICattleRepository
{
    /// <summary>
    ///     Finds an animal with its stage history in the ranch, including inactive animals.
    /// </summary>
    Task<Cattle?> FindByIdAsync(Guid ranchId, CattleId cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an animal in any ranch without tracking it, for system lookups that authorize the result themselves.
    /// </summary>
    Task<Cattle?> FindByIdAsNoTrackingAsync(CattleId cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the animal that holds an ear tag in the ranch, including inactive animals that still reserve it.
    /// </summary>
    Task<CattleId?> FindIdByEarTagAsync(Guid ranchId, EarTag earTag, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns one page of the ranch's animals ordered by ear tag, then identifier.
    /// </summary>
    Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tracks a new animal; the unit of work commits it.
    /// </summary>
    Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default);
}
