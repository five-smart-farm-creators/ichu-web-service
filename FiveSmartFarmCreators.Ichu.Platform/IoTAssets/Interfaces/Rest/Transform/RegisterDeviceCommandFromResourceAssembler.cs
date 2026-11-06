using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class RegisterDeviceCommandFromResourceAssembler
{
    public static RegisterDeviceCommand ToCommandFromResource(Guid ranchId, RegisterDeviceResource resource) =>
        new(ranchId, resource.Serial, resource.Type, resource.FirmwareVersion);
}
