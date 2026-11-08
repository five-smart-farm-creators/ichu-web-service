namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     It is a historical observation, not a live location.
/// </summary>
public record LastKnownPositionResource(
    string Status,
    double? Latitude,
    double? Longitude,
    double? AccuracyMeters,
    DateTimeOffset? RecordedAt,
    bool? IsStale);
