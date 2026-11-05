namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
/// A captured GPS position with its declared accuracy in meters, an invalid capture with its reason, or no capture.
/// </summary>
public sealed record PositionMeasurement
{
    public double? Latitude { get; }
    public double? Longitude { get; }
    public double? AccuracyMeters { get; }
    public string? InvalidReason { get; }

    private PositionMeasurement(double? latitude, double? longitude, double? accuracyMeters, string? invalidReason)
    {
        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        InvalidReason = invalidReason;
    }

    public MeasurementStatus Status => Latitude is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    public bool IsValid => Latitude is not null;

    public static PositionMeasurement Absent() => new(null, null, null, null);

    public static PositionMeasurement From(double? latitude, double? longitude, double? accuracyMeters, string? invalidReason)
    {
        var hasCoordinates = latitude is not null || longitude is not null || accuracyMeters is not null;
        if (hasCoordinates && !string.IsNullOrWhiteSpace(invalidReason))
        {
            throw MonitoringException.InvalidInput("A position cannot be valid and invalid at the same time.");
        }

        if (!hasCoordinates)
        {
            return string.IsNullOrWhiteSpace(invalidReason)
                ? Absent()
                : new PositionMeasurement(null, null, null, Measurement.NormalizeReason(invalidReason));
        }

        if (latitude is not { } lat || longitude is not { } lon || accuracyMeters is not { } accuracy)
        {
            throw MonitoringException.InvalidInput("A valid position requires latitude, longitude and accuracy.");
        }

        if (!GeoCoordinate.IsValidLatitude(lat) || !GeoCoordinate.IsValidLongitude(lon))
        {
            throw MonitoringException.InvalidInput("Position coordinates are outside the valid latitude or longitude range.");
        }

        if (!double.IsFinite(accuracy) || accuracy < 0)
        {
            throw MonitoringException.InvalidInput("Position accuracy must be a non-negative number of meters.");
        }

        return new PositionMeasurement(lat, lon, accuracy, null);
    }
}
