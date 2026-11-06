namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Query to list the geofences defined by a ranch.
/// </summary>
/// <param name="RanchId">The ranch that owns the geofences.</param>
public record GetGeofencesByRanchQuery(Guid RanchId);
