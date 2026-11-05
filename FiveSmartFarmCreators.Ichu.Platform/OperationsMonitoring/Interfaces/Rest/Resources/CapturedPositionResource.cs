using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>Either a position in decimal degrees with accuracy in meters, or the reason the capture is invalid.</summary>
public sealed record CapturedPositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string? InvalidReason)
{
    public CapturedPosition ToCapturedPosition() => new(Latitude, Longitude, AccuracyMeters, InvalidReason);
}
