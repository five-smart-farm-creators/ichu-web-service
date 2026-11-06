using System.Data;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="MonitoringRuleSet"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public class MonitoringRuleSetRepository(DbContext context) : IMonitoringRuleSetRepository
{
    /// <inheritdoc />
    public async Task AddAsync(MonitoringRuleSet ruleSet, CancellationToken cancellationToken = default)
    {
        await context.Set<MonitoringRuleSet>().AddAsync(ruleSet, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MonitoringRuleSet?> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        context.Set<MonitoringRuleSet>().SingleOrDefaultAsync(r => r.RanchId == ranchId, cancellationToken);

    /// <inheritdoc />
    public async Task<MonitoringRuleSnapshot> ReadSnapshotAsync(Guid ranchId, CancellationToken cancellationToken = default)
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
}
