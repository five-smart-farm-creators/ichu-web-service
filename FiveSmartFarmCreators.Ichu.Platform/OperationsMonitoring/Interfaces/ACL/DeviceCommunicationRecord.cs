namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.ACL;

/// <summary>
///     The latest reading synchronized for a device.
/// </summary>
/// <param name="DeviceId">The device identifier.</param>
/// <param name="LastRecordedAt">The capture time of the latest reading.</param>
/// <param name="LastReceivedAt">The instant the cloud received the latest reading.</param>
/// <param name="BatteryPercent">The battery level of that reading, or <see langword="null"/> when it had no valid level.</param>
public record DeviceCommunicationRecord(Guid DeviceId, DateTimeOffset LastRecordedAt, DateTimeOffset LastReceivedAt, double? BatteryPercent);
