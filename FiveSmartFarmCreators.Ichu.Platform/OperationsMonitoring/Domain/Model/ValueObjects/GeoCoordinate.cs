namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

public sealed record GeoCoordinate
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoCoordinate(double latitude, double longitude)
    {
        if (!IsValidLatitude(latitude) || !IsValidLongitude(longitude))
        {
            throw MonitoringException.InvalidInput("Coordinates are outside the valid latitude or longitude range.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public static bool IsValidLatitude(double latitude) => double.IsFinite(latitude) && latitude is >= -90 and <= 90;

    public static bool IsValidLongitude(double longitude) => double.IsFinite(longitude) && longitude is >= -180 and <= 180;
}
