using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class AlertResourceFromEntityAssembler
{
    public static AlertResource ToResourceFromEntity(MonitoringAlert entity) =>
        new(
            entity.Id.Value,
            entity.RanchId,
            entity.CattleId,
            entity.DeviceId,
            entity.Type.ToString(),
            entity.Severity.ToString(),
            entity.Status.ToString(),
            MonitoringUnits.EdgeGatewayOrigin,
            entity.GatewayId,
            entity.RaisedAt,
            entity.ReceivedAt,
            entity.DetectedValue,
            UnitOf(entity.Type),
            entity.ThresholdMinValue,
            entity.ThresholdMaxValue,
            entity.ConfigurationRevision,
            entity.GeofenceId?.Value,
            entity.Latitude,
            entity.Longitude,
            entity.DistanceToBoundaryMeters,
            entity.ClearedAt,
            entity.ExitDuration?.TotalSeconds,
            entity.AcknowledgedAt,
            entity.AcknowledgedBy,
            entity.Finding,
            entity.Action,
            entity.ResponseTime?.TotalSeconds,
            entity.Version,
            entity.Evidence.Select(e => e.ReadingId).ToList());

    private static string? UnitOf(AlertType type) => type switch
    {
        AlertType.Temperature => MonitoringUnits.Celsius,
        AlertType.Activity => MonitoringUnits.Index,
        AlertType.HeartRate => MonitoringUnits.BeatsPerMinute,
        _ => null
    };
}
