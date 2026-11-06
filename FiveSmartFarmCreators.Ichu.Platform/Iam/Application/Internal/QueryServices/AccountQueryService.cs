using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>Resolves the current account and its authorized ranches.</summary>
public sealed class AccountQueryService(ICurrentAccount currentAccount, IAuthorizationQueryService authorization) : IAccountQueryService
{
    /// <inheritdoc />
    public Task<UserAccount> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default) =>
        currentAccount.GetAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchAccess>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default)
    {
        var account = await currentAccount.GetAsync(cancellationToken);
        return await authorization.HandleAsync(new GetAccessibleRanchesQuery(account.Id), cancellationToken);
    }
}
