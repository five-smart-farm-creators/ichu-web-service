namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

/// <summary>
///     Defines the basic persistence operations shared by the repositories of every bounded context.
/// </summary>
/// <typeparam name="TEntity">The entity type handled by the repository.</typeparam>
public interface IBaseRepository<TEntity>
{
    /// <summary>
    ///     Adds an entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task that completes when the entity is tracked.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an entity by its identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The tracked entity, or null when it does not exist.</returns>
    Task<TEntity?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds an entity by its identifier without change tracking.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The untracked entity, or null when it does not exist.</returns>
    Task<TEntity?> FindByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Marks an entity as modified.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    ///     Removes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);

    /// <summary>
    ///     Lists every entity of the repository.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The tracked entities.</returns>
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Lists every entity of the repository without change tracking.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The untracked entities.</returns>
    Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default);
}
