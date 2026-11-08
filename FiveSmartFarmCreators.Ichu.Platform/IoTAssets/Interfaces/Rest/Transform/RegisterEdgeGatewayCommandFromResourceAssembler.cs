using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class RegisterEdgeGatewayCommandFromResourceAssembler
{
    public static RegisterEdgeGatewayCommand ToCommandFromResource(Guid ranchId, RegisterEdgeGatewayResource resource) =>
        new(ranchId, resource.Name);
}
