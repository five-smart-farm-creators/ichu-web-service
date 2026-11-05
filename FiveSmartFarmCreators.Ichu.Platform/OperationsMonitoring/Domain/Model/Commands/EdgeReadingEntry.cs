namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record EdgeReadingEntry(
    Guid ReadingId,
    Guid DeviceId,
    DateTimeOffset? RecordedAt,
    CapturedMetric? BodyTemperature,
    CapturedMetric? ActivityIndex,
    CapturedMetric? RuminationMinutes,
    CapturedMetric? BatteryPercent,
    CapturedPosition? Position);
