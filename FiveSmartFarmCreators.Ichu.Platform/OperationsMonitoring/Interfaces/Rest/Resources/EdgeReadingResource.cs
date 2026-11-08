namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Body temperature is in degrees Celsius, activity in index units, rumination in minutes and battery level in percent. An omitted metric is recorded as absent.
/// </summary>
public record EdgeReadingResource(
    Guid ReadingId,
    Guid DeviceId,
    DateTimeOffset? RecordedAt,
    CapturedMetricResource? BodyTemperature,
    CapturedMetricResource? ActivityIndex,
    CapturedMetricResource? RuminationMinutes,
    CapturedMetricResource? BatteryLevel,
    CapturedPositionResource? Position);
