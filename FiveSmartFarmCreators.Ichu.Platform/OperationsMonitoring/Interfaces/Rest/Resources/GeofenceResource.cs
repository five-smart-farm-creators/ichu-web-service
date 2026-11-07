namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a geofence.
/// </summary>
/// <param name="GeofenceId">The geofence identifier.</param>
/// <param name="RanchId">The ranch that owns the lot.</param>
/// <param name="LotId">The lot the geofence applies to.</param>
/// <param name="Name">The geofence name.</param>
/// <param name="ToleranceMeters">The maximum declared GPS accuracy in meters.</param>
/// <param name="Severity">The severity of the alerts raised when an animal leaves the area.</param>
/// <param name="Revision">The rule revision that last changed the geofence.</param>
/// <param name="Vertices">The boundary vertices.</param>
public record GeofenceResource(
    Guid GeofenceId,
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    string Severity,
    long Revision,
    IReadOnlyList<CoordinateResource> Vertices);
