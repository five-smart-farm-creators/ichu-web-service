using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record GeofenceResource(
    Guid GeofenceId,
    Guid RanchId,
    Guid LotId,
    string Name,
    int ToleranceMeters,
    string Severity,
    long Revision,
    IReadOnlyList<CoordinateResource> Vertices)
{
    public static GeofenceResource From(Geofence geofence) =>
        new(
            geofence.Id,
            geofence.RanchId,
            geofence.LotId,
            geofence.Name,
            geofence.ToleranceMeters,
            geofence.Severity.ToString(),
            geofence.Revision,
            geofence.Boundary().Select(v => new CoordinateResource(v.Latitude, v.Longitude)).ToList());
}
