using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Defines the permitted grazing area of a lot. A lot has at most one geofence.
/// </summary>
public record DefineGeofenceCommand(
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
