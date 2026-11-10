using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

public interface IAuthorizationQueryService
{
    Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default);
}
