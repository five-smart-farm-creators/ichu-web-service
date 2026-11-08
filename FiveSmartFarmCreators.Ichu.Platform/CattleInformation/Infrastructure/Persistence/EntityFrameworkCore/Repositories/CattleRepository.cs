using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Tracks animals and their stage history in the shared context, filtering every query by ranch.
/// </summary>
public sealed class CattleRepository(AppDbContext context) : ICattleRepository
{
    /// <inheritdoc />
    public Task<Cattle?> FindByIdAsync(Guid ranchId, CattleId cattleId, CancellationToken cancellationToken = default) =>
        context.Set<Cattle>()
            .Include(cattle => cattle.StagePeriods)
            .FirstOrDefaultAsync(cattle => cattle.RanchId == ranchId && cattle.Id == cattleId, cancellationToken);

    /// <inheritdoc />
    public async Task<CattleId?> FindIdByEarTagAsync(Guid ranchId, EarTag earTag, CancellationToken cancellationToken = default)
    {
        var holder = await context.Set<Cattle>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cattle => cattle.RanchId == ranchId && cattle.EarTag == earTag, cancellationToken);
        return holder?.Id;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default)
    {
        var cattleQuery = context.Set<Cattle>()
            .Include(cattle => cattle.StagePeriods)
            .Where(cattle => cattle.RanchId == query.RanchId);

        if (!string.IsNullOrWhiteSpace(query.EarTag))
        {
            var earTag = new EarTag(query.EarTag);
            cattleQuery = cattleQuery.Where(cattle => cattle.EarTag == earTag);
        }

        if (query.Status is { } status)
        {
            cattleQuery = cattleQuery.Where(cattle => cattle.Status == status);
        }

        if (query.LotId is { } lotId)
        {
            cattleQuery = cattleQuery.Where(cattle => context.Set<LotMembership>().Any(membership =>
                membership.RanchId == query.RanchId && membership.LotId == lotId && membership.CattleId == cattle.Id));
        }

        return await cattleQuery
            .OrderBy(cattle => cattle.EarTag)
            .ThenBy(cattle => cattle.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default)
    {
        await context.Set<Cattle>().AddAsync(cattle, cancellationToken);
    }
}
