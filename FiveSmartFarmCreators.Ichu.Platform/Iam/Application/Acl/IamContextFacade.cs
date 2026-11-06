using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Acl;

/// <summary>
///     Exposes IAM capabilities through application queries.
/// </summary>
public sealed class IamContextFacade(IAuthorizationQueryService authorization) : IIamContextFacade
{
    /// <inheritdoc />
    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new CheckRanchAccessQuery(accountId, ranchId, scope), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new GetAccessibleRanchesQuery(accountId), cancellationToken);
}
