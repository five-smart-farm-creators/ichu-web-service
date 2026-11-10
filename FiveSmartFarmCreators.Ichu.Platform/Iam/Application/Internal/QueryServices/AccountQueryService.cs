using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

public sealed class AccountQueryService(ICurrentAccount currentAccount, IAuthorizationQueryService authorization) : IAccountQueryService
{
    /// <inheritdoc />
    public Task<Result<UserAccount>> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default) =>
        currentAccount.GetAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RanchAccess>>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default)
    {
        var account = await currentAccount.GetAsync(cancellationToken);
        if (account.IsFailure)
        {
            return Result<IReadOnlyList<RanchAccess>>.Failure(account.DomainError);
        }

        return Result<IReadOnlyList<RanchAccess>>.Success(
            await authorization.HandleAsync(new GetAccessibleRanchesQuery(account.Value!.Id.Value), cancellationToken));
    }
}
