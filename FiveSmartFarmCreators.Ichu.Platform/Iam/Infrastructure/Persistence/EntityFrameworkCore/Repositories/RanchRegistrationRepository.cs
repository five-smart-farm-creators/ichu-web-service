using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores ranch registrations and their memberships. The ranch key is a Guid, so the shared base repository supplies the common additions.
/// </summary>
public sealed class RanchRegistrationRepository(AppDbContext context) : BaseRepository<RanchRegistration>(context), IRanchRegistrationRepository
{
    /// <inheritdoc />
    public Task<bool> ExistsAsync(Guid ranchId, CancellationToken cancellationToken) =>
        Context.Set<RanchRegistration>().AnyAsync(ranch => ranch.Id == ranchId, cancellationToken);

    /// <inheritdoc />
    public Task<bool> ExistsByAccessCodeAsync(RanchAccessCode accessCode, CancellationToken cancellationToken) =>
        Context.Set<RanchRegistration>().AnyAsync(ranch => ranch.AccessCode == accessCode, cancellationToken);

    /// <inheritdoc />
    public Task<RanchRegistration?> FindByAccessCodeAsNoTrackingAsync(RanchAccessCode accessCode, CancellationToken cancellationToken) =>
        Context.Set<RanchRegistration>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ranch => ranch.AccessCode == accessCode, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchRegistration>> FindByIdsAsNoTrackingAsync(IReadOnlyCollection<Guid> ranchIds,
        CancellationToken cancellationToken) =>
        await Context.Set<RanchRegistration>()
            .AsNoTracking()
            .Where(ranch => ranchIds.Contains(ranch.Id))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddMembershipAsync(RanchMembership membership, CancellationToken cancellationToken)
    {
        await Context.Set<RanchMembership>().AddAsync(membership, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RanchMembership>> FindMembershipsByAccountAsync(UserAccountId accountId, CancellationToken cancellationToken) =>
        await Context.Set<RanchMembership>()
            .AsNoTracking()
            .Where(membership => membership.UserAccountId == accountId)
            .ToListAsync(cancellationToken);
}
