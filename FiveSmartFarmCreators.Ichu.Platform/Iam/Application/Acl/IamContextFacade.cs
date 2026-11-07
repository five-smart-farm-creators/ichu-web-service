using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Acl;

/// <summary>
///     Exposes IAM capabilities through application queries.
/// </summary>
public sealed class IamContextFacade(IAuthorizationQueryService authorization, ICurrentAccount currentAccount) : IIamContextFacade
{
    /// <inheritdoc />
    public async Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var account = await currentAccount.GetAsync(cancellationToken);
            return account.Id;
        }
        catch (IamException error) when (error.Code == "unauthorized")
        {
            // Other contexts translate a missing account into their own unauthenticated failure.
            return null;
        }
    }

    /// <inheritdoc />
    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new CheckRanchAccessQuery(accountId, ranchId, scope), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new GetAccessibleRanchesQuery(accountId), cancellationToken);
}
