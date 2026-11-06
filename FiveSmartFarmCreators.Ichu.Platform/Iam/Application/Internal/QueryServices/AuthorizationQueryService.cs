using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>Defines the Authorization Query Service contract.</summary>
public sealed class AuthorizationQueryService(IIamRepository repository, AuthorizationDomainService authorization, TimeProvider clock) : IAuthorizationQueryService
{
    /// <inheritdoc />
    public async Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default)
    {
        var accountId = query.AccountId;
        var account = await repository.FindAccountAsync(accountId, cancellationToken);
        if (account is null) return false;
        return authorization.CanAccessRanch(account, query.RanchId, query.Scope,
            await repository.FindMembershipsAsync(accountId, cancellationToken),
            await repository.FindAdvisoriesAsync(query.RanchId, accountId, cancellationToken), clock.GetUtcNow());
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default)
    {
        var accountId = query.AccountId;
        var account = await repository.FindAccountAsync(accountId, cancellationToken);
        var now = clock.GetUtcNow();
        if (account is null || !account.IsActive(now)) return [];
        var memberships = await repository.FindMembershipsAsync(accountId, cancellationToken);
        var result = memberships.Select(m => new RanchAccess(m.RanchId,
            m.IsOwner && account.Role == UserRole.RanchManager ? AccessScope.Manage : AccessScope.Read, null)).ToList();
        if (account.Role == UserRole.Veterinarian && account.Profile is not null)
        {
            var advisories = await repository.FindVeterinarianAdvisoriesAsync(accountId, cancellationToken);
            result.AddRange(advisories.Where(a => a.IsValidOn(now)).Select(a => new RanchAccess(a.RanchId, a.Scope, a.ExpiresAt)));
        }
        return result.GroupBy(r => r.RanchId).Select(g => g.OrderByDescending(r => r.Scope).First()).ToList();
    }
}
