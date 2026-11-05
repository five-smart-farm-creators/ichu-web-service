using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IMonitoringAlertRepository
{
    Task<MonitoringAlert?> FindAsync(Guid alertId, CancellationToken cancellationToken);

    Task<MonitoringAlert?> FindByOperationAsync(Guid operationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<MonitoringAlert>> FindPendingAsync(Guid cattleId, AlertType type, CancellationToken cancellationToken);

    Task<IReadOnlyList<MonitoringAlert>> FindByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, int skip, int take, CancellationToken cancellationToken);

    Task<int> CountByRanchAsync(Guid ranchId, AlertStatus? status, Guid? cattleId, CancellationToken cancellationToken);

    /// <summary>Finds alerts raised before the period end that were not yet acknowledged when the period started.</summary>
    Task<IReadOnlyList<MonitoringAlert>> FindActiveDuringAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken);

    void Add(MonitoringAlert alert);
}
