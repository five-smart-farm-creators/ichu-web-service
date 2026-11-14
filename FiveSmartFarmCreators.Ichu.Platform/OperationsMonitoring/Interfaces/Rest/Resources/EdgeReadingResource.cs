namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Body temperature is in degrees Celsius, heart rate in beats per minute, activity in index units, rumination in minutes and battery level in percent. The methane index is a relative value from 0 to 100, not parts per million. An omitted metric is recorded as absent.
/// </summary>
public record EdgeReadingResource(
    Guid ReadingId,
    Guid DeviceId,
    DateTimeOffset? RecordedAt,
    CapturedMetricResource? BodyTemperature,
    CapturedMetricResource? ActivityIndex,
    CapturedMetricResource? HeartRate,
    CapturedMetricResource? MethaneIndex,
    CapturedMetricResource? RuminationMinutes,
    CapturedMetricResource? BatteryLevel,
    CapturedPositionResource? Position);
