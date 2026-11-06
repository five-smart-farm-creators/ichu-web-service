using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="MonitoringAlert"/> aggregates.
/// </summary>
public static class AlertResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="MonitoringAlert"/> into an <see cref="AlertResource"/>.
    /// </summary>
    /// <param name="entity">The alert aggregate root.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static AlertResource ToResourceFromEntity(MonitoringAlert entity) =>
        new(
            entity.Id,
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
            entity.GeofenceId,
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
        _ => null
    };
}
