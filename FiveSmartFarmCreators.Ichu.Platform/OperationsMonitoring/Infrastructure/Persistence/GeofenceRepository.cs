using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class GeofenceRepository(DbContext context) : IGeofenceRepository
{
    private DbSet<Geofence> Geofences => context.Set<Geofence>();

    public Task<Geofence?> FindAsync(Guid geofenceId, CancellationToken cancellationToken) =>
        Geofences.SingleOrDefaultAsync(g => g.Id == geofenceId, cancellationToken);

    public Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken) =>
        Geofences.SingleOrDefaultAsync(g => g.RanchId == ranchId && g.LotId == lotId, cancellationToken);

    public async Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken) =>
        await Geofences.Where(g => g.RanchId == ranchId).OrderBy(g => g.Name).AsNoTracking().ToListAsync(cancellationToken);

    public void Add(Geofence geofence) => Geofences.Add(geofence);
}
