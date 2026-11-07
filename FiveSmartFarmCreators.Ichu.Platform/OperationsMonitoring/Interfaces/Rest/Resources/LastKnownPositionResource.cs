namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the last position captured for an animal. It is a historical observation, not a live location.
/// </summary>
/// <param name="Status">Available or Unavailable.</param>
/// <param name="Latitude">The latitude in decimal degrees.</param>
/// <param name="Longitude">The longitude in decimal degrees.</param>
/// <param name="AccuracyMeters">The declared accuracy in meters.</param>
/// <param name="RecordedAt">The capture time of the position.</param>
/// <param name="IsStale">Whether the capture is too old to be presented as current.</param>
public record LastKnownPositionResource(
    string Status,
    double? Latitude,
    double? Longitude,
    double? AccuracyMeters,
    DateTimeOffset? RecordedAt,
    bool? IsStale);
