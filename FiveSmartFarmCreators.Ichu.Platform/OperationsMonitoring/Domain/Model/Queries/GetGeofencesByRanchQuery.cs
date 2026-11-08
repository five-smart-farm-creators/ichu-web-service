namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

/// <summary>
///     Lists the geofences defined by a ranch.
/// </summary>
public record GetGeofencesByRanchQuery(Guid RanchId);
