namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the new name, tolerance, severity and boundary of an existing geofence.
/// </summary>
/// <param name="Name">The geofence name.</param>
/// <param name="ToleranceMeters">The maximum declared GPS accuracy in meters.</param>
/// <param name="Severity">The severity: Informative, Warning or Critical.</param>
/// <param name="Vertices">The boundary vertices.</param>
public record RedefineGeofenceResource(
    string? Name,
    int? ToleranceMeters,
    string? Severity,
    IReadOnlyList<CoordinateResource>? Vertices);
