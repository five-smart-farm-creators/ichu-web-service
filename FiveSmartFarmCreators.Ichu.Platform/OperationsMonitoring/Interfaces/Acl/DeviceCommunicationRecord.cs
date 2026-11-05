namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>The latest reading synchronized for a device. BatteryPercent is null when that reading had no valid battery level.</summary>
public sealed record DeviceCommunicationRecord(Guid DeviceId, DateTimeOffset LastRecordedAt, DateTimeOffset LastReceivedAt, double? BatteryPercent);
