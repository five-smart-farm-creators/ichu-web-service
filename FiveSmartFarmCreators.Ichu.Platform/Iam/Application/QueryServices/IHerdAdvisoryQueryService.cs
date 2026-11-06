using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>
///     Provides authorized read access to ranch advisory history.
/// </summary>
public interface IHerdAdvisoryQueryService
{
    /// <summary>
    ///     Returns advisory history visible to the current account.
    /// </summary>
    /// <returns>
    ///     The authorized history with advisory and veterinarian profile details.
    /// </returns>
    Task<IReadOnlyList<HerdAdvisoryDetails>> HandleAsync(GetHerdAdvisoriesByRanchQuery query, CancellationToken cancellationToken = default);
}
