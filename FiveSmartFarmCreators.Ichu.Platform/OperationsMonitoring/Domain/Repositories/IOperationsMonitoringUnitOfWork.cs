using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Extends the shared unit of work so that a rejected synchronization entry can be undone without saving it.
/// </summary>
public interface IOperationsMonitoringUnitOfWork : IUnitOfWork
{
    /// <summary>
    ///     Discards the pending changes of the monitoring entities, so that one rejected batch entry does not affect the next one.
    /// </summary>
    void DiscardChanges();
}
