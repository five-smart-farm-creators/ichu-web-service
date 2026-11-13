using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Tracks ranch lots and their memberships in the shared context, with a trusted lookup for a lot's owning ranch.
/// </summary>
public sealed class LotRepository(AppDbContext context) : ILotRepository
{
    /// <inheritdoc />
    public Task<Lot?> FindByIdAsync(Guid ranchId, LotId lotId, CancellationToken cancellationToken = default) =>
        context.Set<Lot>()
            .Include(lot => lot.Memberships)
            .FirstOrDefaultAsync(lot => lot.RanchId == ranchId && lot.Id == lotId, cancellationToken);

    /// <inheritdoc />
    public Task<Guid?> FindRanchIdByIdAsync(LotId lotId, CancellationToken cancellationToken = default) =>
        context.Set<Lot>()
            .AsNoTracking()
            .Where(lot => lot.Id == lotId)
            .Select(lot => (Guid?)lot.RanchId)
            .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        await context.Set<Lot>()
            .Include(lot => lot.Memberships)
            .Where(lot => lot.RanchId == ranchId)
            .OrderBy(lot => lot.Name)
            .ThenBy(lot => lot.Id)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<LotMembership>> FindMembershipsByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        await context.Set<LotMembership>()
            .AsNoTracking()
            .Where(membership => membership.RanchId == ranchId)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task AddAsync(Lot lot, CancellationToken cancellationToken = default)
    {
        await context.Set<Lot>().AddAsync(lot, cancellationToken);
    }
}
