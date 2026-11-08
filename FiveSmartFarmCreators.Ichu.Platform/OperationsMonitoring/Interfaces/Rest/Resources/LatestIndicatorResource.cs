namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Status is Available or Unavailable; an unavailable indicator has no value and is never reported as zero or normal. IsStale is true when the capture is older than 24 hours.
/// </summary>
public record LatestIndicatorResource(string Status, double? Value, string Unit, DateTimeOffset? RecordedAt, bool? IsStale);
