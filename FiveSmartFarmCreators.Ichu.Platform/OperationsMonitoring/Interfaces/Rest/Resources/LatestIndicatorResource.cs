namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the latest valid value of one indicator. Status is Available or Unavailable; an
///     unavailable indicator has no value and is never reported as zero or normal. IsStale is true when the capture
///     is older than 24 hours.
/// </summary>
/// <param name="Status">Available or Unavailable.</param>
/// <param name="Value">The latest valid value.</param>
/// <param name="Unit">The unit of the value.</param>
/// <param name="RecordedAt">The capture time of the value.</param>
/// <param name="IsStale">Whether the capture is too old to be presented as current.</param>
public record LatestIndicatorResource(string Status, double? Value, string Unit, DateTimeOffset? RecordedAt, bool? IsStale);
