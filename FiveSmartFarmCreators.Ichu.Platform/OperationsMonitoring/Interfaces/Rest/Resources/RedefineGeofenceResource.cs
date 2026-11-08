namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record RedefineGeofenceResource(
    string? Name,
    int? ToleranceMeters,
    string? Severity,
    IReadOnlyList<CoordinateResource>? Vertices);
