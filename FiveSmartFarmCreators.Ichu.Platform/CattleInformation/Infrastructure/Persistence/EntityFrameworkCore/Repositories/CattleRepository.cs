using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class CattleRepository<TContext> : ICattleRepository where TContext : DbContext
{
    private readonly TContext _context;

    public CattleRepository(TContext context)
    {
        _context = context;
    }

    public Task<Cattle?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        return _context.Set<Cattle>()
            .Include(cattle => cattle.StagePeriods)
            .SingleOrDefaultAsync(cattle => cattle.RanchId == ranchId && cattle.Id == cattleId, cancellationToken);
    }

    public Task<Cattle?> FindByEarTagAsync(Guid ranchId, string earTag, CancellationToken cancellationToken)
    {
        var normalizedEarTag = new EarTag(earTag);
        return _context.Set<Cattle>()
            .Include(cattle => cattle.StagePeriods)
            .SingleOrDefaultAsync(cattle => cattle.RanchId == ranchId && cattle.EarTag == normalizedEarTag,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query,
        CancellationToken cancellationToken)
    {
        var cattleQuery = _context.Set<Cattle>()
            .Include(cattle => cattle.StagePeriods)
            .Where(cattle => cattle.RanchId == query.RanchId);

        if (!string.IsNullOrWhiteSpace(query.EarTag))
        {
            var earTag = new EarTag(query.EarTag);
            cattleQuery = cattleQuery.Where(cattle => cattle.EarTag == earTag);
        }

        if (query.Status.HasValue)
        {
            cattleQuery = cattleQuery.Where(cattle => cattle.Status == query.Status.Value);
        }

        if (query.LotId.HasValue)
        {
            cattleQuery = cattleQuery.Where(cattle => _context.Set<LotMembership>().Any(membership =>
                membership.RanchId == query.RanchId && membership.LotId == query.LotId.Value &&
                membership.CattleId == cattle.Id));
        }

        return await cattleQuery
            .OrderBy(cattle => cattle.EarTag)
            .ThenBy(cattle => cattle.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Cattle cattle, CancellationToken cancellationToken)
    {
        await _context.Set<Cattle>().AddAsync(cattle, cancellationToken);
    }
}
