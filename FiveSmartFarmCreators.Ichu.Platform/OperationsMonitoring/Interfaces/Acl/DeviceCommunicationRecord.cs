namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     The latest reading synchronized for a device.
/// </summary>
public record DeviceCommunicationRecord(Guid DeviceId, DateTimeOffset LastRecordedAt, DateTimeOffset LastReceivedAt, double? BatteryPercent);
