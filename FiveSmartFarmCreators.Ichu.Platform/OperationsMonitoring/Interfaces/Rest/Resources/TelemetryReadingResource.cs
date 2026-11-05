using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record TelemetryReadingResource(
    Guid ReadingId,
    Guid DeviceId,
    Guid? AnimalId,
    Guid GatewayId,
    string Origin,
    DateTimeOffset RecordedAt,
    DateTimeOffset ReceivedAt,
    MeasurementResource BodyTemperature,
    MeasurementResource ActivityIndex,
    MeasurementResource RuminationMinutes,
    MeasurementResource BatteryLevel,
    PositionResource Position)
{
    public static TelemetryReadingResource From(TelemetryReading reading) =>
        new(
            reading.ReadingId,
            reading.DeviceId,
            reading.CattleId,
            reading.GatewayId,
            MonitoringUnits.EdgeGatewayOrigin,
            reading.RecordedAt,
            reading.ReceivedAt,
            MeasurementResource.From(reading.BodyTemperature, MonitoringUnits.Celsius),
            MeasurementResource.From(reading.ActivityIndex, MonitoringUnits.Index),
            MeasurementResource.From(reading.RuminationMinutes, MonitoringUnits.Minutes),
            MeasurementResource.From(reading.BatteryPercent, MonitoringUnits.Percent),
            PositionResource.From(reading.Position));
}
