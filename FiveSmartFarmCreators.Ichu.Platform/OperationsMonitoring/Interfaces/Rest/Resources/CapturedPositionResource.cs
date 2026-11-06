namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a position sent by an Edge Gateway: either decimal degrees with accuracy in meters, or the reason the capture is invalid.
/// </summary>
/// <param name="Latitude">The latitude in decimal degrees.</param>
/// <param name="Longitude">The longitude in decimal degrees.</param>
/// <param name="AccuracyMeters">The declared accuracy in meters.</param>
/// <param name="InvalidReason">The reason the capture is invalid.</param>
public record CapturedPositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string? InvalidReason);
