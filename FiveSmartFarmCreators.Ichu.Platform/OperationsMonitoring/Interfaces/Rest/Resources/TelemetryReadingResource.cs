namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record TelemetryReadingResource(
    Guid ReadingId,
    Guid DeviceId,
    Guid? CattleId,
    Guid GatewayId,
    string Origin,
    DateTimeOffset RecordedAt,
    DateTimeOffset ReceivedAt,
    MeasurementResource BodyTemperature,
    MeasurementResource ActivityIndex,
    MeasurementResource RuminationMinutes,
    MeasurementResource BatteryLevel,
    PositionResource Position);
