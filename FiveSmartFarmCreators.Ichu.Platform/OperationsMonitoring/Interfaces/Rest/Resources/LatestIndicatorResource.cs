using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// The latest valid value of one indicator. Status is Available or Unavailable; an unavailable indicator has
/// no value and is never reported as zero or normal. IsStale is true when the capture is older than 24 hours.
/// </summary>
public sealed record LatestIndicatorResource(string Status, double? Value, string Unit, DateTimeOffset? RecordedAt, bool? IsStale)
{
    public static LatestIndicatorResource From(TelemetryReading? reading, Func<TelemetryReading, Measurement> select, string unit, DateTimeOffset now) =>
        reading is null
            ? new LatestIndicatorResource("Unavailable", null, unit, null, null)
            : new LatestIndicatorResource("Available", select(reading).Value, unit, reading.RecordedAt, reading.IsStale(now));
}
