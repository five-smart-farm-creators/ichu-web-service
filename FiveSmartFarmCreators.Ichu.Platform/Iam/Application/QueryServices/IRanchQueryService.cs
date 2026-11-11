using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

public interface IRanchQueryService
{
    Task<Result<RanchRegistration>> HandleAsync(GetRanchByAccessCodeQuery query, CancellationToken cancellationToken = default);
}
