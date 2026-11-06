namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a captured GPS position with its declared accuracy in meters, an invalid capture
///     with its reason, or no capture.
/// </summary>
public record PositionMeasurement
{
    private PositionMeasurement(double? latitude, double? longitude, double? accuracyMeters, string? invalidReason)
    {
        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        InvalidReason = invalidReason;
    }

    /// <summary>Gets the latitude in decimal degrees, when the capture is valid.</summary>
    public double? Latitude { get; }

    /// <summary>Gets the longitude in decimal degrees, when the capture is valid.</summary>
    public double? Longitude { get; }

    /// <summary>Gets the declared accuracy in meters, when the capture is valid.</summary>
    public double? AccuracyMeters { get; }

    /// <summary>Gets the reason the device marked the capture as invalid, if it did.</summary>
    public string? InvalidReason { get; }

    /// <summary>Gets the quality of the capture.</summary>
    public MeasurementStatus Status => Latitude is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    /// <summary>Gets a value indicating whether the capture carries coordinates.</summary>
    public bool IsValid => Latitude is not null;

    /// <summary>
    ///     Creates a position for a capture the device did not send.
    /// </summary>
    /// <returns>An absent position.</returns>
    public static PositionMeasurement Absent() => new(null, null, null, null);

    /// <summary>
    ///     Creates a position from the data sent by a device.
    /// </summary>
    /// <param name="latitude">The latitude in decimal degrees, if any.</param>
    /// <param name="longitude">The longitude in decimal degrees, if any.</param>
    /// <param name="accuracyMeters">The declared accuracy in meters, if any.</param>
    /// <param name="invalidReason">The reason the capture is invalid, if the device reported one.</param>
    /// <returns>A valid, invalid or absent position.</returns>
    /// <exception cref="ArgumentException">Thrown when the data is contradictory, incomplete or outside its valid range.</exception>
    public static PositionMeasurement From(double? latitude, double? longitude, double? accuracyMeters, string? invalidReason)
    {
        var hasCoordinates = latitude is not null || longitude is not null || accuracyMeters is not null;
        if (hasCoordinates && !string.IsNullOrWhiteSpace(invalidReason))
        {
            throw new ArgumentException("A position cannot be valid and invalid at the same time.");
        }

        if (!hasCoordinates)
        {
            return string.IsNullOrWhiteSpace(invalidReason)
                ? Absent()
                : new PositionMeasurement(null, null, null, Measurement.NormalizeReason(invalidReason));
        }

        if (latitude is not { } lat || longitude is not { } lon || accuracyMeters is not { } accuracy)
        {
            throw new ArgumentException("A valid position requires latitude, longitude and accuracy.");
        }

        if (!GeoCoordinate.IsValidLatitude(lat) || !GeoCoordinate.IsValidLongitude(lon))
        {
            throw new ArgumentException("Position coordinates are outside the valid latitude or longitude range.");
        }

        if (!double.IsFinite(accuracy) || accuracy < 0)
        {
            throw new ArgumentException("Position accuracy must be a non-negative number of meters.");
        }

        return new PositionMeasurement(lat, lon, accuracy, null);
    }
}
