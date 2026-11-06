namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record AssignDeviceCommand(Guid RanchId, Guid DeviceId, Guid TargetId, Guid ExpectedVersion);
