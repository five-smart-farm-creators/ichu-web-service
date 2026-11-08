namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Saves the monitoring changes as one unit. A concurrent or duplicate change is reported as a result instead of an exception, so that one rejected batch entry never aborts the entries after it.
/// </summary>
public interface IOperationsMonitoringUnitOfWork
{
    /// <summary>
    ///     Saves the pending monitoring changes and returns false when a concurrent change conflicts with them. The pending changes are discarded in that case.
    /// </summary>
    Task<bool> TryCompleteAsync(CancellationToken cancellationToken = default);
}
