using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores accounts in the shared context. It is not derived from the base repository, whose lookups take Guid keys.
/// </summary>
public sealed class UserAccountRepository(AppDbContext context) : IUserAccountRepository
{
    /// <inheritdoc />
    public Task<UserAccount?> FindByIdAsNoTrackingAsync(UserAccountId accountId, CancellationToken cancellationToken) =>
        context.Set<UserAccount>()
            .AsNoTracking()
            .FirstOrDefaultAsync(account => account.Id == accountId, cancellationToken);

    /// <inheritdoc />
    public Task<UserAccount?> FindByExternalUidAsync(string externalUid, CancellationToken cancellationToken) =>
        context.Set<UserAccount>()
            .FirstOrDefaultAsync(account => account.ExternalUid == externalUid, cancellationToken);

    /// <inheritdoc />
    public Task<UserAccount?> FindByEmailAsync(Email email, CancellationToken cancellationToken) =>
        context.Set<UserAccount>()
            .FirstOrDefaultAsync(account => account.Email == email, cancellationToken);

    /// <inheritdoc />
    public Task<bool> LicenseExistsAsync(string licenseNumber, UserAccountId exceptAccountId, CancellationToken cancellationToken) =>
        context.Set<UserAccount>()
            .AnyAsync(account => account.Id != exceptAccountId && account.Profile != null &&
                account.Profile.LicenseNumber == licenseNumber, cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        await context.Set<UserAccount>().AddAsync(account, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAccessAttemptAsync(AccessAttempt attempt, CancellationToken cancellationToken)
    {
        await context.Set<AccessAttempt>().AddAsync(attempt, cancellationToken);
    }
}
