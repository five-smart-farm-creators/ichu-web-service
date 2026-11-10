using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>
///     Evaluates ranch-access queries with the repositories and the authorization rules. Read-only, so nothing is tracked.
/// </summary>
public sealed class AuthorizationQueryService(
    IUserAccountRepository accounts,
    IRanchRegistrationRepository ranches,
    IHerdAdvisoryRepository advisories,
    AuthorizationDomainService authorization,
    TimeProvider clock) : IAuthorizationQueryService
{
    /// <inheritdoc />
    public async Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default)
    {
        var accountId = new UserAccountId(query.AccountId);
        var account = await accounts.FindByIdAsNoTrackingAsync(accountId, cancellationToken);
        if (account is null)
        {
            return false;
        }

        return authorization.CanAccessRanch(account, query.RanchId, query.Scope,
            await ranches.FindMembershipsByAccountAsync(accountId, cancellationToken),
            await advisories.FindByRanchAsNoTrackingAsync(query.RanchId, accountId, cancellationToken),
            clock.GetUtcNow());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default)
    {
        var accountId = new UserAccountId(query.AccountId);
        var account = await accounts.FindByIdAsNoTrackingAsync(accountId, cancellationToken);
        var now = clock.GetUtcNow();
        if (account is null || !account.IsActive(now))
        {
            return [];
        }

        var memberships = await ranches.FindMembershipsByAccountAsync(accountId, cancellationToken);
        var result = memberships
            .Select(membership => new RanchAccess(membership.RanchId,
                membership.IsOwner && account.Role == UserRole.RanchManager ? AccessScope.Manage : AccessScope.Read, null))
            .ToList();
        if (account.Role == UserRole.Veterinarian && account.Profile is not null)
        {
            var veterinarianAdvisories = await advisories.FindByVeterinarianAsNoTrackingAsync(accountId, cancellationToken);
            result.AddRange(veterinarianAdvisories
                .Where(advisory => advisory.IsValidOn(now))
                .Select(advisory => new RanchAccess(advisory.RanchId, advisory.Scope, advisory.ExpiresAt)));
        }

        return result
            .GroupBy(access => access.RanchId)
            .Select(group => group.OrderByDescending(access => access.Scope).First())
            .ToList();
    }
}
