using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

public interface IAccountQueryService
{
    Task<Result<UserAccount>> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<RanchAccess>>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default);
}
