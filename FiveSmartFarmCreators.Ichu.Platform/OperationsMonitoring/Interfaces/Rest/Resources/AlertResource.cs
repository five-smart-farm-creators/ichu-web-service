namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing an alert with its stable identity. RaisedAt is the capture time of the triggering
///     reading; durations are in seconds and measured from that event time, never from the cloud reception time.
/// </summary>
/// <param name="AlertId">The stable alert identifier.</param>
/// <param name="RanchId">The ranch that owns the animal.</param>
/// <param name="AnimalId">The animal the alert refers to.</param>
/// <param name="DeviceId">The device that captured the triggering reading.</param>
/// <param name="Type">The condition that raised the alert.</param>
/// <param name="Severity">The alert severity.</param>
/// <param name="Status">Pending or Acknowledged.</param>
/// <param name="Origin">The component that produced the alert.</param>
/// <param name="GatewayId">The Edge Gateway that evaluated the alert.</param>
/// <param name="RaisedAt">The capture time of the triggering reading.</param>
/// <param name="ReceivedAt">The instant the cloud received the alert.</param>
/// <param name="DetectedValue">The value that exceeded the threshold.</param>
/// <param name="Unit">The unit of the detected value.</param>
/// <param name="ThresholdMinValue">The lower bound of the evaluated threshold.</param>
/// <param name="ThresholdMaxValue">The upper bound of the evaluated threshold.</param>
/// <param name="ConfigurationRevision">The rule revision used to evaluate the alert.</param>
/// <param name="GeofenceId">The geofence that was left.</param>
/// <param name="Latitude">The latitude of the triggering position.</param>
/// <param name="Longitude">The longitude of the triggering position.</param>
/// <param name="DistanceToBoundaryMeters">The distance from the triggering position to the boundary in meters.</param>
/// <param name="ClearedAt">The instant the animal returned to the permitted area.</param>
/// <param name="ExitDurationSeconds">The seconds between the alert event and the return.</param>
/// <param name="AcknowledgedAt">The instant the alert received attention.</param>
/// <param name="AcknowledgedBy">The account that attended the alert.</param>
/// <param name="Finding">The finding recorded with the attention.</param>
/// <param name="Action">The action recorded with the attention.</param>
/// <param name="ResponseTimeSeconds">The seconds between the alert event and its attention.</param>
/// <param name="Version">The version used to detect stale attention requests.</param>
/// <param name="ReadingIds">The readings that support the alert.</param>
public record AlertResource(
    Guid AlertId,
    Guid RanchId,
    Guid AnimalId,
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
