namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

/// <summary>
///     Commits the pending changes of the repositories in one transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Saves every pending change.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the changes are saved.</returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}
