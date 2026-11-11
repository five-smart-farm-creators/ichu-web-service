using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.Iam.Application;

internal sealed class FakeUserAccounts : IUserAccountRepository
{
    public List<UserAccount> Accounts { get; } = [];

    public List<AccessAttempt> Attempts { get; } = [];

    public Task<UserAccount?> FindByIdAsNoTrackingAsync(UserAccountId accountId, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.SingleOrDefault(account => account.Id == accountId));

    public Task<UserAccount?> FindByExternalUidAsync(string externalUid, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.SingleOrDefault(account => account.ExternalUid == externalUid));

    public Task<UserAccount?> FindByEmailAsync(Email email, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.SingleOrDefault(account => account.Email == email));

    public Task<bool> LicenseExistsAsync(string licenseNumber, UserAccountId exceptAccountId, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.Any(account => account.Id != exceptAccountId && account.Profile?.LicenseNumber == licenseNumber));

    public Task AddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        Accounts.Add(account);
        return Task.CompletedTask;
    }

    public Task AddAccessAttemptAsync(AccessAttempt attempt, CancellationToken cancellationToken)
    {
        Attempts.Add(attempt);
        return Task.CompletedTask;
    }
}

internal sealed class FakeRanches : IRanchRegistrationRepository
{
    public List<RanchRegistration> Ranches { get; } = [];

    public List<RanchMembership> Memberships { get; } = [];

    public Task<bool> ExistsAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Task.FromResult(Ranches.Any(ranch => ranch.Id == ranchId));

    public Task<bool> ExistsByAccessCodeAsync(RanchAccessCode accessCode, CancellationToken cancellationToken) =>
        Task.FromResult(Ranches.Any(ranch => ranch.AccessCode == accessCode));

    public Task<RanchRegistration?> FindByAccessCodeAsNoTrackingAsync(RanchAccessCode accessCode, CancellationToken cancellationToken) =>
        Task.FromResult(Ranches.SingleOrDefault(ranch => ranch.AccessCode == accessCode));

    public Task<IReadOnlyList<RanchRegistration>> FindByIdsAsNoTrackingAsync(IReadOnlyCollection<Guid> ranchIds,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RanchRegistration>>(Ranches.Where(ranch => ranchIds.Contains(ranch.Id)).ToList());

    public Task AddAsync(RanchRegistration ranch, CancellationToken cancellationToken)
    {
        Ranches.Add(ranch);
        return Task.CompletedTask;
    }

    public Task AddMembershipAsync(RanchMembership membership, CancellationToken cancellationToken)
    {
        Memberships.Add(membership);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RanchMembership>> FindMembershipsByAccountAsync(UserAccountId accountId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RanchMembership>>(Memberships.Where(membership => membership.UserAccountId == accountId).ToList());
}

internal sealed class FakeIdentityProvider : IIdentityProvider
{
    public Result<ExternalIdentity> VerifyResult { get; set; } = Result<ExternalIdentity>.Failure(IamErrors.InvalidCredentials);

    public Result<string> SignInResult { get; set; } = Result<string>.Success("id-token");

    public int SignInCalls { get; private set; }

    public Task<Result<ExternalIdentity>> VerifyIdTokenAsync(string idToken, CancellationToken cancellationToken) =>
        Task.FromResult(VerifyResult);

    public Task<Result<string>> SignInAsync(Email email, string password, CancellationToken cancellationToken)
    {
        SignInCalls++;
        return Task.FromResult(SignInResult);
    }
}

internal sealed class FakeCurrentAccount : ICurrentAccount
{
    public Result<ExternalIdentity> Identity { get; set; } =
        Result<ExternalIdentity>.Failure(IamErrors.Unauthenticated);

    public Result<UserAccount> Account { get; set; } = Result<UserAccount>.Failure(IamErrors.AccountNotActive);

    public Task<Result<UserAccount>> GetAsync(CancellationToken cancellationToken) => Task.FromResult(Account);

    public Result<ExternalIdentity> GetExternalIdentity() => Identity;
}

internal sealed class CountingUnitOfWork : IUnitOfWork
{
    public int CompletedCount { get; private set; }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        CompletedCount++;
        return Task.CompletedTask;
    }
}

internal sealed class NoAuthorization : IAuthorizationQueryService
{
    public Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RanchAccess>>([]);
}

internal sealed class FixedAuthorization(params RanchAccess[] accesses) : IAuthorizationQueryService
{
    public Task<bool> HandleAsync(CheckRanchAccessQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<IReadOnlyList<RanchAccess>> HandleAsync(GetAccessibleRanchesQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<RanchAccess>>(accesses);
}

/// <summary>
///     Returns the scripted codes in order and then keeps returning the last one, so that a test can force collisions.
/// </summary>
internal sealed class FakeRanchAccessCodeGenerator(params string[] codes) : IRanchAccessCodeGenerator
{
    private readonly Queue<string> _codes = new(codes);

    public int Calls { get; private set; }

    public RanchAccessCode Next()
    {
        Calls++;
        return new RanchAccessCode(_codes.Count > 1 ? _codes.Dequeue() : _codes.Peek());
    }
}
