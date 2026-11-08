using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A geographic point in decimal degrees.
/// </summary>
public record GeoCoordinate
{
    private GeoCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    public static Result<GeoCoordinate> Create(double latitude, double longitude) =>
        IsValidLatitude(latitude) && IsValidLongitude(longitude)
            ? Result<GeoCoordinate>.Success(new GeoCoordinate(latitude, longitude))
            : Result<GeoCoordinate>.Failure(OperationsMonitoringErrors.CoordinateOutOfRange);

    public static bool IsValidLatitude(double latitude) => double.IsFinite(latitude) && latitude is >= -90 and <= 90;

    public static bool IsValidLongitude(double longitude) => double.IsFinite(longitude) && longitude is >= -180 and <= 180;
}
