using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to define the permitted grazing area of a lot.
/// </summary>
/// <param name="RanchId">The ranch that owns the lot.</param>
/// <param name="LotId">The lot the geofence applies to.</param>
/// <param name="Name">The geofence name.</param>
/// <param name="ToleranceMeters">The maximum declared GPS accuracy, in meters, for a position to be conclusive.</param>
/// <param name="Severity">The severity of the alerts raised when an animal leaves the area.</param>
/// <param name="Boundary">The boundary vertices.</param>
public sealed record DefineGeofenceCommand(
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
