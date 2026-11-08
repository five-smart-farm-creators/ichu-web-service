using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record SetDeviceMaintenanceCommand(Guid RanchId, DeviceId DeviceId, bool InMaintenance, Guid ExpectedVersion);
