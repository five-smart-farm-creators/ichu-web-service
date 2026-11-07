using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Domain repository interface for managing <see cref="Geofence"/> aggregates.
/// </summary>
public interface IGeofenceRepository
{
    /// <summary>
    ///     Adds a new geofence asynchronously to the repository.
    /// </summary>
    /// <param name="geofence">The geofence to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous add operation.</returns>
    Task AddAsync(Geofence geofence, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a geofence by its identifier asynchronously.
    /// </summary>
    /// <param name="geofenceId">The geofence identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the geofence if found; otherwise, <see langword="null"/>.</returns>
    Task<Geofence?> FindByIdAsync(Guid geofenceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the geofence of a lot asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the lot.</param>
    /// <param name="lotId">The lot the geofence applies to.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the geofence if found; otherwise, <see langword="null"/>.</returns>
    Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the geofences of a ranch ordered by name asynchronously.
    /// </summary>
    /// <param name="ranchId">The ranch that owns the geofences.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the geofences of the ranch.</returns>
    Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
