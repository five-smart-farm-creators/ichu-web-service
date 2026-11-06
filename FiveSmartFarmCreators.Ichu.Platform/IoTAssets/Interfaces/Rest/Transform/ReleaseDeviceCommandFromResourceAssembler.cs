using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class ReleaseDeviceCommandFromResourceAssembler
{
    public static ReleaseDeviceCommand ToCommandFromResource(Guid ranchId, Guid deviceId, ReleaseDeviceResource resource) =>
        new(ranchId, deviceId, resource.Reason, resource.ExpectedVersion);
}
