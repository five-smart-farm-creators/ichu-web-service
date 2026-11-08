using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
///     Reads ranch lots after checking the caller's ranch read access.
/// </summary>
public interface ILotQueryService
{
    /// <summary>
    ///     Lists the ranch's lots with their stored memberships.
    /// </summary>
    Task<Result<IReadOnlyList<Lot>>> HandleAsync(GetLotsByRanchQuery query, CancellationToken cancellationToken);
}
