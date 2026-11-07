using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="Geofence"/> aggregates.
/// </summary>
public static class GeofenceResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="Geofence"/> into a <see cref="GeofenceResource"/>.
    /// </summary>
    /// <param name="entity">The geofence aggregate root.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static GeofenceResource ToResourceFromEntity(Geofence entity) =>
        new(
            entity.Id,
            entity.RanchId,
            entity.LotId,
            entity.Name,
            entity.ToleranceMeters,
            entity.Severity.ToString(),
            entity.Revision,
            entity.Boundary().Select(v => new CoordinateResource(v.Latitude, v.Longitude)).ToList());
}
