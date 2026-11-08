using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class GeofenceResourceFromEntityAssembler
{
    public static GeofenceResource ToResourceFromEntity(Geofence entity) =>
        new(
            entity.Id.Value,
            entity.RanchId,
            entity.LotId,
            entity.Name,
            entity.ToleranceMeters,
            entity.Severity.ToString(),
            entity.Revision,
            entity.Boundary().Select(v => new CoordinateResource(v.Latitude, v.Longitude)).ToList());
}
