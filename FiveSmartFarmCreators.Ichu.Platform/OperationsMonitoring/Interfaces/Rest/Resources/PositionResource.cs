using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>A captured position in decimal degrees with its declared accuracy in meters.</summary>
public sealed record PositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string Status, string? InvalidReason)
{
    public static PositionResource From(PositionMeasurement position) =>
        new(position.Latitude, position.Longitude, position.AccuracyMeters, position.Status.ToString(), position.InvalidReason);
}
