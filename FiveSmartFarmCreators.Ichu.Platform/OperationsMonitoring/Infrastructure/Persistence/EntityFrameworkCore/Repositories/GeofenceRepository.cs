using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="Geofence"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public class GeofenceRepository(DbContext context) : IGeofenceRepository
{
    private DbSet<Geofence> Geofences => context.Set<Geofence>();

    /// <inheritdoc />
    public async Task AddAsync(Geofence geofence, CancellationToken cancellationToken = default)
    {
        await Geofences.AddAsync(geofence, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Geofence?> FindByIdAsync(Guid geofenceId, CancellationToken cancellationToken = default) =>
        Geofences.SingleOrDefaultAsync(g => g.Id == geofenceId, cancellationToken);

    /// <inheritdoc />
    public Task<Geofence?> FindByLotAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default) =>
        Geofences.SingleOrDefaultAsync(g => g.RanchId == ranchId && g.LotId == lotId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Geofence>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default) =>
        await Geofences.Where(g => g.RanchId == ranchId).OrderBy(g => g.Name).AsNoTracking().ToListAsync(cancellationToken);
}
