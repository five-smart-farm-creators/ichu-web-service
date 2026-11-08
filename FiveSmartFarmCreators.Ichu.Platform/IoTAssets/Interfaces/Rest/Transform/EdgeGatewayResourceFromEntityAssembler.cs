using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class EdgeGatewayResourceFromEntityAssembler
{
    public static EdgeGatewayResource ToResourceFromEntity(EdgeGateway gateway) =>
        new(gateway.Id.Value, gateway.RanchId, gateway.Name, gateway.Status.ToString(), gateway.RegisteredAt, gateway.RevokedAt);

    public static EdgeGatewayRegistrationResource ToRegistrationResourceFromEntity(EdgeGatewayRegistration registration) =>
        new(registration.Gateway.Id.Value, registration.Gateway.RanchId, registration.Gateway.Name,
            registration.Gateway.Status.ToString(), registration.Gateway.RegisteredAt, registration.Key);
}
