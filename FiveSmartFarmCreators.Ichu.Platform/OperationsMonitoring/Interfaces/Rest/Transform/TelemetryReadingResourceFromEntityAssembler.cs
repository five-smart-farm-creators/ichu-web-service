using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class TelemetryReadingResourceFromEntityAssembler
{
    public static TelemetryReadingResource ToResourceFromEntity(TelemetryReading entity) =>
        new(
            entity.ReadingId,
            entity.DeviceId,
            entity.CattleId,
            entity.GatewayId,
            MonitoringUnits.EdgeGatewayOrigin,
            entity.RecordedAt,
            entity.ReceivedAt,
            ToMeasurementResource(entity.BodyTemperature, MonitoringUnits.Celsius),
            ToMeasurementResource(entity.ActivityIndex, MonitoringUnits.Index),
            ToMeasurementResource(entity.HeartRate, MonitoringUnits.BeatsPerMinute),
            ToMeasurementResource(entity.MethaneIndex, MonitoringUnits.Index),
            ToMeasurementResource(entity.RuminationMinutes, MonitoringUnits.Minutes),
            ToMeasurementResource(entity.BatteryPercent, MonitoringUnits.Percent),
            new PositionResource(
                entity.Position.Latitude,
                entity.Position.Longitude,
                entity.Position.AccuracyMeters,
                entity.Position.Status.ToString(),
                entity.Position.InvalidReason));

    private static MeasurementResource ToMeasurementResource(Measurement measurement, string unit) =>
        new(measurement.Value, unit, measurement.Status.ToString(), measurement.InvalidReason);
}
