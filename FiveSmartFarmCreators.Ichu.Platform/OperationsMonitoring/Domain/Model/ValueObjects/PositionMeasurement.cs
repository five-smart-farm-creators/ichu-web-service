using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A GPS position with its declared accuracy, an explicitly invalid capture with its reason, or no capture at all.
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

    public double? Latitude { get; }

    public double? Longitude { get; }

    public double? AccuracyMeters { get; }

    public string? InvalidReason { get; }

    public MeasurementStatus Status => Latitude is not null
        ? MeasurementStatus.Valid
        : InvalidReason is not null ? MeasurementStatus.Invalid : MeasurementStatus.Absent;

    public bool IsValid => Latitude is not null;

    public static PositionMeasurement Absent() => new(null, null, null, null);

    public static Result<PositionMeasurement> From(double? latitude, double? longitude, double? accuracyMeters, string? invalidReason)
    {
        var hasCoordinates = latitude is not null || longitude is not null || accuracyMeters is not null;
        if (hasCoordinates && !string.IsNullOrWhiteSpace(invalidReason))
        {
            return Result<PositionMeasurement>.Failure(OperationsMonitoringErrors.PositionValidAndInvalid);
        }

        if (!hasCoordinates)
        {
            if (string.IsNullOrWhiteSpace(invalidReason))
            {
                return Result<PositionMeasurement>.Success(Absent());
            }

            var reason = Measurement.NormalizeReason(invalidReason);
            return reason.IsFailure
                ? Result<PositionMeasurement>.Failure(reason.DomainError)
                : Result<PositionMeasurement>.Success(new PositionMeasurement(null, null, null, reason.Value));
        }

        if (latitude is not { } lat || longitude is not { } lon || accuracyMeters is not { } accuracy)
        {
            return Result<PositionMeasurement>.Failure(OperationsMonitoringErrors.PositionIncomplete);
        }

        if (!GeoCoordinate.IsValidLatitude(lat) || !GeoCoordinate.IsValidLongitude(lon))
        {
            return Result<PositionMeasurement>.Failure(OperationsMonitoringErrors.PositionCoordinatesOutOfRange);
        }

        if (!double.IsFinite(accuracy) || accuracy < 0)
        {
            return Result<PositionMeasurement>.Failure(OperationsMonitoringErrors.PositionAccuracyInvalid);
        }

        return Result<PositionMeasurement>.Success(new PositionMeasurement(lat, lon, accuracy, null));
    }
}
