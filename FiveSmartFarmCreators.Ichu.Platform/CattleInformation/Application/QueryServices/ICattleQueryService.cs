using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
///     Reads animal information after checking the caller's ranch read access.
/// </summary>
public interface ICattleQueryService
{
    /// <summary>
    ///     Combines an animal's identity and stage history with its device details.
    /// </summary>
    Task<Result<CattleProfile>> HandleAsync(GetCattleByIdQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns one page of matching animals in the caller's ranch.
    /// </summary>
    Task<Result<IReadOnlyList<Cattle>>> HandleAsync(SearchCattleQuery query, CancellationToken cancellationToken);
}
