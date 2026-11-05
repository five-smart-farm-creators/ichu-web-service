using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record RedefineGeofenceCommand(
    Guid GeofenceId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
