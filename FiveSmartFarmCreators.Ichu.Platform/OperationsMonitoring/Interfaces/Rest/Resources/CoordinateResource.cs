using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record CoordinateResource(double? Latitude, double? Longitude)
{
    public GeoCoordinate ToCoordinate() =>
        Latitude is { } latitude && Longitude is { } longitude
            ? new GeoCoordinate(latitude, longitude)
            : throw MonitoringException.InvalidInput("Every vertex requires a latitude and a longitude.");
}
