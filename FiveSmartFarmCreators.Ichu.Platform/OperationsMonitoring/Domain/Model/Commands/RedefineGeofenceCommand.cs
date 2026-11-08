using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Replaces the name, tolerance, severity and boundary of an existing geofence.
/// </summary>
public record RedefineGeofenceCommand(
    GeofenceId GeofenceId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
