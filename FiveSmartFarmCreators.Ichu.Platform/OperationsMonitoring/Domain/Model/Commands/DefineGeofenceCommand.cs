using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record DefineGeofenceCommand(
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    AlertSeverity Severity,
    IReadOnlyList<GeoCoordinate> Boundary);
