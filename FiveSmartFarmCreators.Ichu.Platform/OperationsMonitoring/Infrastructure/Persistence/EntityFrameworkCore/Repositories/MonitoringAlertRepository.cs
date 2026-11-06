using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="MonitoringAlert"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public sealed class MonitoringAlertRepository(DbContext context) : IMonitoringAlertRepository
{
    private DbSet<MonitoringAlert> Alerts => context.Set<MonitoringAlert>();

    /// <inheritdoc />
    public async Task AddAsync(MonitoringAlert alert, CancellationToken cancellationToken = default)
    {
        await Alerts.AddAsync(alert, cancellationToken);
    }

    /// <inheritdoc />
    public Task<MonitoringAlert?> FindByIdAsync(Guid alertId, CancellationToken cancellationToken = default) =>
        Alerts.SingleOrDefaultAsync(a => a.Id == alertId, cancellationToken);

    /// <inheritdoc />
    public Task<MonitoringAlert?> FindByOperationIdAsync(Guid operationId, CancellationToken cancellationToken = default) =>
        Alerts.SingleOrDefaultAsync(a => a.Changes.Any(c => c.OperationId == operationId), cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringAlert>> FindPendingByCattleAndTypeAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken = default) =>
        await Alerts
            .Where(a => a.CattleId == cattleId && a.Type == type && a.Status == AlertStatus.Pending && a.ClearedAt == null)
            .OrderBy(a => a.RaisedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(
        Guid ranchId,
        AlertStatus? status,
        Guid? cattleId,
        int skip,
        int take,
        CancellationToken cancellationToken = default) =>
        await Filter(ranchId, status, cattleId)
            .OrderByDescending(a => a.RaisedAt)
            .ThenBy(a => a.Id)
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken = default) =>
        Filter(ranchId, status, cattleId).CountAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
        await Alerts
            .Where(a => a.RanchId == ranchId && a.RaisedAt < period.To && (a.AcknowledgedAt == null || a.AcknowledgedAt >= period.From))
            .OrderBy(a => a.RaisedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

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
