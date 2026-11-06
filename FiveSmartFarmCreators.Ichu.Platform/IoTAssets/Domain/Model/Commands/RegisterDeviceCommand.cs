using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record RegisterDeviceCommand(Guid RanchId, string Serial, DeviceType Type, string? FirmwareVersion);
