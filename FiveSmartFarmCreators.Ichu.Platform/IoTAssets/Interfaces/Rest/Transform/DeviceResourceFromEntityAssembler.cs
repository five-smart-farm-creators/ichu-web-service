using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(IoTDevice device, DateTimeOffset now, int? lowBatteryThreshold)
    {
        var currentAssignment = device.Assignments.SingleOrDefault(a => a.IsOpen());
        bool? isBatteryLow = device.Battery is not null && lowBatteryThreshold is int threshold
            ? device.Battery.IsLow(threshold) : null;
        return new DeviceResource(device.Id.Value, device.RanchId, device.Serial.Value, device.Type.ToString(),
            device.Status.ToString(), device.IsAvailable(), device.GetCommunicationStatus(now).ToString(),
            device.LastCommunicationAt, device.LastCommunicationReceivedAt, device.Battery?.Percentage,
            device.BatteryRecordedAt, isBatteryLow, device.FirmwareVersion, device.Version,
            currentAssignment is null ? null : DeviceAssignmentResourceFromEntityAssembler.ToResourceFromEntity(currentAssignment));
    }
}
