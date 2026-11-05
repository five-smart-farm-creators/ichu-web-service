using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class LivestockOperationRepository(DbContext context) : ILivestockOperationRepository
{
    private DbSet<LivestockOperation> Operations => context.Set<LivestockOperation>();

    public Task<LivestockOperation?> FindAsync(Guid operationId, CancellationToken cancellationToken) =>
        Operations.AsNoTracking().SingleOrDefaultAsync(o => o.Id == operationId, cancellationToken);

    public async Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken) =>
        await Operations
            .Where(o => o.CattleId == cattleId && o.OccurredAt >= period.From && o.OccurredAt < period.To)
            .OrderBy(o => o.OccurredAt)
            .ThenBy(o => o.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void Add(LivestockOperation operation) => Operations.Add(operation);
}
