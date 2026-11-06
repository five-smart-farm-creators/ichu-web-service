using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;

/// <summary>
///     Provides server-side authorization queries for other bounded contexts.
/// </summary>
public interface IAuthorizationQueryService
{
    /// <summary>
    ///     Checks current access for a trusted server-side actor identifier.
    /// </summary>
    /// <returns>
    ///     <see langword="true"/> when the account has the requested ranch scope.
    /// </returns>
    Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns current membership and advisory access for a trusted actor.
    /// </summary>
    /// <returns>
    ///     The effective ranch scopes for the account.
    /// </returns>
    Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default);
}
