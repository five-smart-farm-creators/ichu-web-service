using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

/// <summary>
/// Reads animal information after checking the current caller's ranch access.
/// </summary>
public interface ICattleQueryService
{
    /// <summary>
    /// Combines an authorized animal's identity and stage history with external device availability.
    /// </summary>
    /// <returns>A profile that distinguishes unavailable integration data from an unassigned collar.</returns>
    Task<CattleProfile> HandleAsync(GetCattleByIdQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Searches one authorized ranch, including inactive cattle unless a status filter is supplied.
    /// </summary>
    /// <returns>An ordered page of matching animals, or an empty page when none match.</returns>
    Task<IReadOnlyList<Cattle>> HandleAsync(SearchCattleQuery query, CancellationToken cancellationToken);
}
