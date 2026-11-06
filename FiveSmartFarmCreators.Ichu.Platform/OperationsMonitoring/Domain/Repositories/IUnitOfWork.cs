namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

// Move to Shared/Domain/Repositories when the shared kernel is available.
/// <summary>
///     Defines the Unit of Work contract that commits the pending changes of the monitoring repositories atomically.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Persists all pending changes to the underlying database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Discards the pending changes of the monitoring entities, so that one rejected batch entry does not affect the next one.
    /// </summary>
    void DiscardChanges();
}
