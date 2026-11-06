using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="LivestockOperation"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public sealed class LivestockOperationRepository(DbContext context) : ILivestockOperationRepository
{
    private DbSet<LivestockOperation> Operations => context.Set<LivestockOperation>();

    /// <inheritdoc />
    public async Task AddAsync(LivestockOperation operation, CancellationToken cancellationToken = default)
    {
        await Operations.AddAsync(operation, cancellationToken);
    }

    /// <inheritdoc />
    public Task<LivestockOperation?> FindByIdAsync(Guid operationId, CancellationToken cancellationToken = default) =>
        Operations.AsNoTracking().SingleOrDefaultAsync(o => o.Id == operationId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default) =>
        await Operations
            .Where(o => o.CattleId == cattleId && o.OccurredAt >= period.From && o.OccurredAt < period.To)
            .OrderBy(o => o.OccurredAt)
            .ThenBy(o => o.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
