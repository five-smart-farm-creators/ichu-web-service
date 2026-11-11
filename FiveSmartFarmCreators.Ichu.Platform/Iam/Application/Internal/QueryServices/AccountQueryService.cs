using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

public sealed class AccountQueryService(
    ICurrentAccount currentAccount,
    IAuthorizationQueryService authorization,
    IRanchRegistrationRepository ranches) : IAccountQueryService
{
    /// <inheritdoc />
    public Task<Result<UserAccount>> HandleAsync(GetCurrentAccountQuery query, CancellationToken cancellationToken = default) =>
        currentAccount.GetAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AccountRanch>>> HandleAsync(GetCurrentRanchesQuery query, CancellationToken cancellationToken = default)
    {
        var account = await currentAccount.GetAsync(cancellationToken);
        if (account.IsFailure)
        {
            return Result<IReadOnlyList<AccountRanch>>.Failure(account.DomainError);
        }

        var access = await authorization.HandleAsync(new GetAccessibleRanchesQuery(account.Value!.Id.Value), cancellationToken);
        var registrations = await ranches.FindByIdsAsNoTrackingAsync(access.Select(entry => entry.RanchId).ToList(), cancellationToken);
        var result = access
            .Join(registrations, entry => entry.RanchId, ranch => ranch.Id,
                (entry, ranch) => new AccountRanch(entry, ranch.Name, ranch.Region, ranch.District,
                    entry.Scope == AccessScope.Manage ? ranch.AccessCode : null))
            .ToList();
        return Result<IReadOnlyList<AccountRanch>>.Success(result);
    }
}
