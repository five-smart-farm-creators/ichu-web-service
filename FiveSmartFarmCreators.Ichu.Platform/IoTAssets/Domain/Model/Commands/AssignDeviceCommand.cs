using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record AssignDeviceCommand(Guid RanchId, DeviceId DeviceId, Guid TargetId, Guid ExpectedVersion);
