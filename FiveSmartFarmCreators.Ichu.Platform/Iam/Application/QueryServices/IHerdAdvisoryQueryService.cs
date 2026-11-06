using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>Defines the HerdAdvisoryQueryService application contract.</summary>
public interface IHerdAdvisoryQueryService
{
    /// <summary>Returns advisory history visible to the current account.</summary>
    Task<IReadOnlyList<HerdAdvisoryDetails>> HandleAsync(GetHerdAdvisoriesByRanchQuery query, CancellationToken cancellationToken = default);
}
