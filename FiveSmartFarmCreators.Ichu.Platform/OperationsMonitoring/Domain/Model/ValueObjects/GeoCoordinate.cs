namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a geographic point in decimal degrees.
/// </summary>
public record GeoCoordinate
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GeoCoordinate"/> value object.
    /// </summary>
    /// <param name="latitude">The latitude in decimal degrees, between -90 and 90.</param>
    /// <param name="longitude">The longitude in decimal degrees, between -180 and 180.</param>
    /// <exception cref="ArgumentException">Thrown when a coordinate is not finite or is outside its valid range.</exception>
    public GeoCoordinate(double latitude, double longitude)
    {
        if (!IsValidLatitude(latitude) || !IsValidLongitude(longitude))
        {
            throw new ArgumentException("Coordinates are outside the valid latitude or longitude range.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Gets the latitude in decimal degrees.</summary>
    public double Latitude { get; }

    /// <summary>Gets the longitude in decimal degrees.</summary>
    public double Longitude { get; }

    /// <summary>
    ///     Determines whether a latitude is finite and within the valid range.
    /// </summary>
    /// <param name="latitude">The latitude to check.</param>
    /// <returns><see langword="true"/> when the latitude is valid.</returns>
    public static bool IsValidLatitude(double latitude) => double.IsFinite(latitude) && latitude is >= -90 and <= 90;

    /// <summary>
    ///     Determines whether a longitude is finite and within the valid range.
    /// </summary>
    /// <param name="longitude">The longitude to check.</param>
    /// <returns><see langword="true"/> when the longitude is valid.</returns>
    public static bool IsValidLongitude(double longitude) => double.IsFinite(longitude) && longitude is >= -180 and <= 180;
}
