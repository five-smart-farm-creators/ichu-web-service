using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Acl;

public sealed class IamContextFacade(IAuthorizationQueryService authorization, ICurrentAccount currentAccount) : IIamContextFacade
{
    /// <inheritdoc />
    public async Task<Guid?> FetchCurrentAccountIdAsync(CancellationToken cancellationToken)
    {
        // Other contexts translate a missing account into their own unauthenticated failure.
        var account = await currentAccount.GetAsync(cancellationToken);
        return account.IsSuccess ? account.Value!.Id.Value : null;
    }

    /// <inheritdoc />
    public Task<bool> CanAccessRanchAsync(Guid accountId, Guid ranchId, AccessScope scope, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new CheckRanchAccessQuery(accountId, ranchId, scope), cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyList<RanchAccess>> GetAccessibleRanchesAsync(Guid accountId, CancellationToken cancellationToken) =>
        authorization.HandleAsync(new GetAccessibleRanchesQuery(accountId), cancellationToken);
}
