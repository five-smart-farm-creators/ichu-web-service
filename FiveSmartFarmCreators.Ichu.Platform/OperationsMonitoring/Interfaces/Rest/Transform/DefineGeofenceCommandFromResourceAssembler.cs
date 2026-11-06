using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles the define geofence command from its REST resource.
/// </summary>
public static class DefineGeofenceCommandFromResourceAssembler
{
    /// <summary>
    ///     Transforms a <see cref="DefineGeofenceResource"/> into a <see cref="DefineGeofenceCommand"/>.
    /// </summary>
    /// <param name="resource">The HTTP resource containing the geofence definition.</param>
    /// <returns>The command with validated value objects.</returns>
    /// <exception cref="ArgumentException">Thrown when a value is missing, unknown or violates a value object rule.</exception>
    public static DefineGeofenceCommand ToCommandFromResource(DefineGeofenceResource resource)
    {
        var tolerance = resource.ToleranceMeters ?? throw new ArgumentException("The tolerance in meters is required.");
        var severityName = Enum.GetNames<AlertSeverity>().FirstOrDefault(n => n.Equals(resource.Severity?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (severityName is null)
        {
            throw new ArgumentException($"The severity must be one of: {string.Join(", ", Enum.GetNames<AlertSeverity>())}.");
        }

        var boundary = (resource.Vertices ?? []).Select(ToCoordinate).ToList();
        return new DefineGeofenceCommand(resource.RanchId, resource.LotId, resource.Name ?? string.Empty, tolerance,
            Enum.Parse<AlertSeverity>(severityName), boundary);
    }

    private static GeoCoordinate ToCoordinate(CoordinateResource vertex) =>
        vertex.Latitude is { } latitude && vertex.Longitude is { } longitude
            ? new GeoCoordinate(latitude, longitude)
            : throw new ArgumentException("Every vertex requires a latitude and a longitude.");
}
