namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The tolerance is the maximum declared GPS accuracy, in meters, for a position to be conclusive.
/// </summary>
public record DefineGeofenceResource(
    Guid RanchId,
    Guid LotId,
    string? Name,
    int? ToleranceMeters,
    string? Severity,
    IReadOnlyList<CoordinateResource>? Vertices);
