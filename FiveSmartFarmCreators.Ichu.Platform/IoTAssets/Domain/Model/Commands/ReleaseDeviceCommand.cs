namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record ReleaseDeviceCommand(Guid RanchId, Guid DeviceId, string Reason, Guid ExpectedVersion);
