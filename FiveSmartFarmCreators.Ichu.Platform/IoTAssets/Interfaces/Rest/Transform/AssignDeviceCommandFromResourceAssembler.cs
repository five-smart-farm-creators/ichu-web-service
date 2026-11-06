using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class AssignDeviceCommandFromResourceAssembler
{
    public static AssignDeviceCommand ToCommandFromResource(Guid ranchId, Guid deviceId, AssignDeviceResource resource) =>
        new(ranchId, deviceId, resource.TargetId, resource.ExpectedVersion);
}
