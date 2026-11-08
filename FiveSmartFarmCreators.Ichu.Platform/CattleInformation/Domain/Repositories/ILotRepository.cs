using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

/// <summary>
///     Provides ranch-scoped lot persistence. Callers check access before querying.
/// </summary>
public interface ILotRepository
{
    /// <summary>
    ///     Finds a lot with its stored memberships in the ranch.
    /// </summary>
    Task<Lot?> FindByIdAsync(Guid ranchId, LotId lotId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the ranch's lots ordered by name, then identifier.
    /// </summary>
    Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists the memberships of every lot of the ranch without tracking them, so herd reads avoid loading lots.
    /// </summary>
    Task<IReadOnlyList<LotMembership>> FindMembershipsByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Tracks a new lot; the unit of work commits it.
    /// </summary>
    Task AddAsync(Lot lot, CancellationToken cancellationToken = default);
}
