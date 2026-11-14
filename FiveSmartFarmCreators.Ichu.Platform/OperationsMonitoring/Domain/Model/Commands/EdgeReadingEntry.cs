namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     A reading relayed by an Edge Gateway. A metric the device did not send is recorded as absent.
/// </summary>
public record EdgeReadingEntry(
    Guid ReadingId,
    Guid DeviceId,
    DateTimeOffset? RecordedAt,
    CapturedMetric? BodyTemperature,
    CapturedMetric? ActivityIndex,
    CapturedMetric? HeartRate,
    CapturedMetric? MethaneIndex,
    CapturedMetric? RuminationMinutes,
    CapturedMetric? BatteryPercent,
    CapturedPosition? Position);
