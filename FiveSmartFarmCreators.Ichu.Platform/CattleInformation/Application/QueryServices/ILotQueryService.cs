using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
/// Reads ranch lots after checking the current caller's access.
/// </summary>
public interface ILotQueryService
{
    /// <summary>
    /// Lists stored ranch lots and memberships without filtering inactive animals from those memberships.
    /// </summary>
    /// <returns>The ranch's lots, or an empty list when none exist.</returns>
    Task<IReadOnlyList<Lot>> HandleAsync(GetLotsByRanchQuery query, CancellationToken cancellationToken);
}
