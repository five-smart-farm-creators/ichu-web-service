using System.Data;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class MonitoringRuleSetRepository(DbContext context) : IMonitoringRuleSetRepository
{
    public Task<MonitoringRuleSet?> FindAsync(Guid ranchId, CancellationToken cancellationToken) =>
        context.Set<MonitoringRuleSet>().SingleOrDefaultAsync(r => r.RanchId == ranchId, cancellationToken);

    public async Task<MonitoringRuleSnapshot> ReadSnapshotAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        // Repeatable read gives the three queries one PostgreSQL snapshot, so the revision matches the rules returned.
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken)
            : null;

        var revision = await context.Set<MonitoringRuleSet>()
            .Where(r => r.RanchId == ranchId)
            .Select(r => (long?)r.Revision)
            .SingleOrDefaultAsync(cancellationToken);
        var thresholds = await context.Set<MonitoringThreshold>()
            .Where(t => t.RanchId == ranchId)
            .OrderBy(t => t.LifeStage)
            .ThenBy(t => t.Metric)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var geofences = await context.Set<Geofence>()
            .Where(g => g.RanchId == ranchId)
            .OrderBy(g => g.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        return new MonitoringRuleSnapshot(ranchId, revision ?? 0, thresholds, geofences);
    }

    public void Add(MonitoringRuleSet ruleSet) => context.Set<MonitoringRuleSet>().Add(ruleSet);
}
