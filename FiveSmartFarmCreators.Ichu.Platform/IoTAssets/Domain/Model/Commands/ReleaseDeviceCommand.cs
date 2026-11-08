using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record ReleaseDeviceCommand(Guid RanchId, DeviceId DeviceId, string Reason, Guid ExpectedVersion);
