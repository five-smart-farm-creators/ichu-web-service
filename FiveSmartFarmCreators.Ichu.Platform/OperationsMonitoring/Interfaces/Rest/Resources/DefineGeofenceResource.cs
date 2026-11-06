namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the permitted grazing area of a lot: a boundary of at least three vertices in decimal
///     degrees. The tolerance is the maximum declared GPS accuracy, in meters, for a position to be conclusive.
/// </summary>
/// <param name="RanchId">The ranch that owns the lot.</param>
/// <param name="LotId">The lot the geofence applies to.</param>
/// <param name="Name">The geofence name.</param>
/// <param name="ToleranceMeters">The maximum declared GPS accuracy in meters.</param>
/// <param name="Severity">The severity: Informative, Warning or Critical.</param>
/// <param name="Vertices">The boundary vertices.</param>
public record DefineGeofenceResource(
    Guid RanchId,
    Guid LotId,
    string? Name,
    int? ToleranceMeters,
    string? Severity,
    IReadOnlyList<CoordinateResource>? Vertices);
