using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class MonitoringAlertRepository(DbContext context) : IMonitoringAlertRepository
{
    private DbSet<MonitoringAlert> Alerts => context.Set<MonitoringAlert>();

    public Task<MonitoringAlert?> FindAsync(Guid alertId, CancellationToken cancellationToken) =>
        Alerts.SingleOrDefaultAsync(a => a.Id == alertId, cancellationToken);

    public Task<MonitoringAlert?> FindByOperationAsync(Guid operationId, CancellationToken cancellationToken) =>
        Alerts.SingleOrDefaultAsync(a => a.Changes.Any(c => c.OperationId == operationId), cancellationToken);

    public async Task<IReadOnlyList<MonitoringAlert>> FindPendingAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken) =>
        await Alerts
            .Where(a => a.CattleId == cattleId && a.Type == type && a.Status == AlertStatus.Pending && a.ClearedAt == null)
            .OrderBy(a => a.RaisedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(
        Guid ranchId,
        AlertStatus? status,
        Guid? cattleId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await Filter(ranchId, status, cattleId)
            .OrderByDescending(a => a.RaisedAt)
            .ThenBy(a => a.Id)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken) =>
        Filter(ranchId, status, cattleId).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken) =>
        await Alerts
            .Where(a => a.RanchId == ranchId && a.RaisedAt < period.To && (a.AcknowledgedAt == null || a.AcknowledgedAt >= period.From))
            .OrderBy(a => a.RaisedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public void Add(MonitoringAlert alert) => Alerts.Add(alert);

    private IQueryable<MonitoringAlert> Filter(Guid ranchId, AlertStatus? status, Guid? cattleId)
    {
        var alerts = Alerts.Where(a => a.RanchId == ranchId);
        if (status is { } requiredStatus)
        {
            alerts = alerts.Where(a => a.Status == requiredStatus);
        }

        if (cattleId is { } requiredCattleId)
        {
            alerts = alerts.Where(a => a.CattleId == requiredCattleId);
        }

        return alerts;
    }
}
