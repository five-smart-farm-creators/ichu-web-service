namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;

public sealed record SetDeviceMaintenanceCommand(Guid RanchId, Guid DeviceId, bool InMaintenance, Guid ExpectedVersion);
