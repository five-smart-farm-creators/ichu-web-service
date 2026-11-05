using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// An alert with its stable identity. RaisedAt is the capture time of the triggering reading; durations are in
/// seconds and measured from that event time, never from the cloud reception time.
/// </summary>
public sealed record AlertResource(
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
    IReadOnlyList<Guid> ReadingIds)
{
    public static AlertResource From(MonitoringAlert alert) =>
        new(
            alert.Id,
            alert.RanchId,
            alert.CattleId,
            alert.DeviceId,
            alert.Type.ToString(),
            alert.Severity.ToString(),
            alert.Status.ToString(),
            MonitoringUnits.EdgeGatewayOrigin,
            alert.GatewayId,
            alert.RaisedAt,
            alert.ReceivedAt,
            alert.DetectedValue,
            UnitOf(alert.Type),
            alert.ThresholdMinValue,
            alert.ThresholdMaxValue,
            alert.ConfigurationRevision,
            alert.GeofenceId,
            alert.Latitude,
            alert.Longitude,
            alert.DistanceToBoundaryMeters,
            alert.ClearedAt,
            alert.ExitDuration?.TotalSeconds,
            alert.AcknowledgedAt,
            alert.AcknowledgedBy,
            alert.Finding,
            alert.Action,
            alert.ResponseTime?.TotalSeconds,
            alert.Version,
            alert.Evidence.Select(e => e.ReadingId).ToList());

    private static string? UnitOf(AlertType type) => type switch
    {
        AlertType.Temperature => MonitoringUnits.Celsius,
        AlertType.Activity => MonitoringUnits.Index,
        _ => null
    };
}
