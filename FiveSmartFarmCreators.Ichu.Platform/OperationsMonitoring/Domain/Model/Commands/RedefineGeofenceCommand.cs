using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to replace the name, tolerance, severity and boundary of an existing geofence.
/// </summary>
/// <param name="GeofenceId">The geofence to redefine.</param>
/// <param name="Name">The geofence name.</param>
/// <param name="ToleranceMeters">The maximum declared GPS accuracy, in meters, for a position to be conclusive.</param>
/// <param name="Severity">The severity of the alerts raised when an animal leaves the area.</param>
/// <param name="Boundary">The boundary vertices.</param>
public record RedefineGeofenceCommand(
    Guid GeofenceId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
