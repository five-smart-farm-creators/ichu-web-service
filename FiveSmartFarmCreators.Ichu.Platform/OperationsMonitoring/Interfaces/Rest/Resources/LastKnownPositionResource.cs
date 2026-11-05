using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>The last position captured for the animal. It is a historical observation, not a live location.</summary>
public sealed record LastKnownPositionResource(
    string Status,
    double? Latitude,
    double? Longitude,
    double? AccuracyMeters,
    DateTimeOffset? RecordedAt,
    bool? IsStale)
{
    public static LastKnownPositionResource From(TelemetryReading? reading, DateTimeOffset now) =>
        reading is null
            ? new LastKnownPositionResource("Unavailable", null, null, null, null, null)
            : new LastKnownPositionResource("Available", reading.Position.Latitude, reading.Position.Longitude,
                reading.Position.AccuracyMeters, reading.RecordedAt, reading.IsStale(now));
}
