namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     RaisedAt is the capture time of the triggering reading; durations are in seconds and measured from that event time, never from the cloud reception time.
/// </summary>
public record AlertResource(
    Guid AlertId,
    Guid RanchId,
    Guid CattleId,
    Guid DeviceId,
    string Type,
    string Severity,
    string Status,
    string Origin,
    Guid GatewayId,
    DateTimeOffset RaisedAt,
    DateTimeOffset ReceivedAt,
    double? DetectedValue,
    string? Unit,
    double? ThresholdMinValue,
    double? ThresholdMaxValue,
    long ConfigurationRevision,
    Guid? GeofenceId,
    double? Latitude,
    double? Longitude,
    double? DistanceToBoundaryMeters,
    DateTimeOffset? ClearedAt,
    double? ExitDurationSeconds,
    DateTimeOffset? AcknowledgedAt,
    Guid? AcknowledgedBy,
    string? Finding,
    string? Action,
    double? ResponseTimeSeconds,
    int Version,
    IReadOnlyList<Guid> ReadingIds);
