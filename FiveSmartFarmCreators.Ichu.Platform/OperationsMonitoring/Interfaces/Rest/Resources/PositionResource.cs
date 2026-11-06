namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a captured position in decimal degrees with its declared accuracy in meters.
/// </summary>
/// <param name="Latitude">The latitude in decimal degrees.</param>
/// <param name="Longitude">The longitude in decimal degrees.</param>
/// <param name="AccuracyMeters">The declared accuracy in meters.</param>
/// <param name="Status">Valid, Invalid or Absent.</param>
/// <param name="InvalidReason">The reason the capture is invalid.</param>
public record PositionResource(double? Latitude, double? Longitude, double? AccuracyMeters, string Status, string? InvalidReason);
