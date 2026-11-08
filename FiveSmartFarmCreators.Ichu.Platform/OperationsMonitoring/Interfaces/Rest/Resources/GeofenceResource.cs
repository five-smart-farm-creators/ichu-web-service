namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record GeofenceResource(
    Guid GeofenceId,
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    string Severity,
    long Revision,
    IReadOnlyList<CoordinateResource> Vertices);
