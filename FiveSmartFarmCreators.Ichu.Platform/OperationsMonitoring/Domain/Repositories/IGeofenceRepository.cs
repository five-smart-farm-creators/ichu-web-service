using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores geofences, at most one per lot.
/// </summary>
public interface IGeofenceRepository
{
    Task AddAsync(Geofence geofence, CancellationToken cancellationToken = default);

    Task<Geofence?> FindByIdAsync(GeofenceId geofenceId, CancellationToken cancellationToken = default);

    Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
}
