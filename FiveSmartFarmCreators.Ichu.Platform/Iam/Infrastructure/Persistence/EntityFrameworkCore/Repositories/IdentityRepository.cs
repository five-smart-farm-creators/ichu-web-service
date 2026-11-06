using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Persists IAM records through the configured EF Core context.
/// </summary>
public sealed class IdentityRepository(DbContext context) : IIamRepository
{
    /// <inheritdoc />
    public Task<UserAccount?> FindAccountAsync(Guid id, CancellationToken cancellationToken) =>
        context.Set<UserAccount>().SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    /// <inheritdoc />
    public Task<UserAccount?> FindByExternalUidAsync(string uid, CancellationToken cancellationToken) =>
        context.Set<UserAccount>().SingleOrDefaultAsync(a => a.ExternalUid == uid, cancellationToken);
    /// <inheritdoc />
    public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Set<UserAccount>().SingleOrDefaultAsync(a => a.Email == new Email(email), cancellationToken);
    /// <inheritdoc />
    public Task<bool> LicenseExistsAsync(string license, Guid exceptAccountId, CancellationToken cancellationToken) =>
        context.Set<UserAccount>().AnyAsync(a => a.Id != exceptAccountId && a.Profile != null && a.Profile.LicenseNumber == license, cancellationToken);
    /// <inheritdoc />
    public Task<bool> RanchExistsAsync(Guid ranchId, CancellationToken cancellationToken) =>
        context.Set<RanchRegistration>().AnyAsync(r => r.Id == ranchId, cancellationToken);
    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchMembership>> FindMembershipsAsync(Guid accountId, CancellationToken cancellationToken) =>
        await context.Set<RanchMembership>().AsNoTracking().Where(m => m.UserAccountId == accountId).ToListAsync(cancellationToken);
    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisory>> FindAdvisoriesAsync(Guid ranchId, Guid? veterinarianId, CancellationToken cancellationToken) =>
        await context.Set<HerdAdvisory>().Where(a => a.RanchId == ranchId && (veterinarianId == null || a.VeterinarianId == veterinarianId)).ToListAsync(cancellationToken);
    /// <inheritdoc />
    public Task<HerdAdvisory?> FindAdvisoryAsync(Guid id, CancellationToken cancellationToken) =>
        context.Set<HerdAdvisory>().SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    /// <inheritdoc />
    public async Task<IReadOnlyList<HerdAdvisory>> FindVeterinarianAdvisoriesAsync(Guid accountId, CancellationToken cancellationToken) =>
        await context.Set<HerdAdvisory>().AsNoTracking().Where(a => a.VeterinarianId == accountId).ToListAsync(cancellationToken);
    /// <inheritdoc />
    public void AddAccessAttempt(AccessAttempt attempt) => context.Set<AccessAttempt>().Add(attempt);
    /// <inheritdoc />
    public void AddAccount(UserAccount account) => context.Set<UserAccount>().Add(account);
    /// <inheritdoc />
    public void AddRanch(RanchRegistration ranch) => context.Set<RanchRegistration>().Add(ranch);
    /// <inheritdoc />
    public void AddMembership(RanchMembership membership) => context.Set<RanchMembership>().Add(membership);
    /// <inheritdoc />
    public void AddAdvisory(HerdAdvisory advisory) => context.Set<HerdAdvisory>().Add(advisory);

    /// <inheritdoc />
    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            throw new IamException("conflict", "The record changed. Reload before retrying.");
        }
        catch (DbUpdateException error) when (error.InnerException is PostgresException { SqlState: "23505" })
        {
            throw new IamException("conflict", "A unique identity, license or active advisory already exists.");
        }
    }
}
