using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface IGeofenceRepository
{
    Task<Geofence?> FindAsync(Guid geofenceId, CancellationToken cancellationToken);

    Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken);

    void Add(Geofence geofence);
}
