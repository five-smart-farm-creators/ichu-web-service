using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>Defines the AuthorizationQueryService application contract.</summary>
public interface IAuthorizationQueryService
{
    /// <summary>Checks current access for a trusted server-side actor identifier.</summary>
    Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default);

    /// <summary>Returns current membership and advisory access for a trusted actor.</summary>
    Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default);
}
