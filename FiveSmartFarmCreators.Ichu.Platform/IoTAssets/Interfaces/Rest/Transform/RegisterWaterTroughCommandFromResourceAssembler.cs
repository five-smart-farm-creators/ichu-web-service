using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class RegisterWaterTroughCommandFromResourceAssembler
{
    public static RegisterWaterTroughCommand ToCommandFromResource(Guid ranchId, RegisterWaterTroughResource resource) =>
        new(ranchId, resource.Name, resource.LotId);
}
