using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

public interface IHerdAdvisoryQueryService
{
    Task<Result<IReadOnlyList<HerdAdvisoryDetails>>> HandleAsync(GetHerdAdvisoriesByRanchQuery query, CancellationToken cancellationToken = default);
}
