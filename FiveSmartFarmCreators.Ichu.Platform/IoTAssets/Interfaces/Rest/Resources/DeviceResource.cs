namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record DeviceResource(Guid Id, Guid RanchId, string Serial, string Type, string Status, bool IsAvailable,
    string CommunicationStatus, DateTimeOffset? LastCommunicationAt, DateTimeOffset? LastCommunicationReceivedAt,
    int? BatteryPercentage, DateTimeOffset? BatteryRecordedAt, bool? IsBatteryLow,
    string? FirmwareVersion, Guid Version, DeviceAssignmentResource? CurrentAssignment);
