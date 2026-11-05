using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// A reading relayed by the Edge Gateway. Body temperature is in degrees Celsius, activity in index units,
/// rumination in minutes and battery level in percent. An omitted metric is recorded as absent.
/// </summary>
public sealed record EdgeReadingResource(
    Guid ReadingId,
    Guid DeviceId,
    DateTimeOffset? RecordedAt,
    CapturedMetricResource? BodyTemperature,
    CapturedMetricResource? ActivityIndex,
    CapturedMetricResource? RuminationMinutes,
    CapturedMetricResource? BatteryLevel,
    CapturedPositionResource? Position)
{
    public EdgeReadingEntry ToEntry() =>
        new(
            ReadingId,
            DeviceId,
            RecordedAt,
            BodyTemperature?.ToCapturedMetric(),
            ActivityIndex?.ToCapturedMetric(),
            RuminationMinutes?.ToCapturedMetric(),
            BatteryLevel?.ToCapturedMetric(),
            Position?.ToCapturedPosition());
}
