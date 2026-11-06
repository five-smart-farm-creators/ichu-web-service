using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Tracks ranch lots and memberships in the same host context used by the module's unit of work.
/// </summary>
public sealed class LotRepository<TContext> : ILotRepository where TContext : DbContext
{
    private readonly TContext _context;

    public LotRepository(TContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public Task<Lot?> FindByIdAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken)
    {
        return _context.Set<Lot>()
            .Include(lot => lot.Memberships)
            .SingleOrDefaultAsync(lot => lot.RanchId == ranchId && lot.Id == lotId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        return await _context.Set<Lot>()
            .Include(lot => lot.Memberships)
            .Where(lot => lot.RanchId == ranchId)
            .OrderBy(lot => lot.Name)
            .ThenBy(lot => lot.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Lot lot, CancellationToken cancellationToken)
    {
        await _context.Set<Lot>().AddAsync(lot, cancellationToken);
    }
}
