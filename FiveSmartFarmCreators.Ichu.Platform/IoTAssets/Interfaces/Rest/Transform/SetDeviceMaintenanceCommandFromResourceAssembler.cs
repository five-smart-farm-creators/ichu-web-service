using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class SetDeviceMaintenanceCommandFromResourceAssembler
{
    public static SetDeviceMaintenanceCommand ToCommandFromResource(Guid ranchId, Guid deviceId, SetDeviceMaintenanceResource resource) =>
        new(ranchId, deviceId, resource.InMaintenance, resource.ExpectedVersion);
}
