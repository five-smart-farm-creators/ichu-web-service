using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class ConfigureWaterTroughTemperatureCommandFromResourceAssembler
{
    public static ConfigureWaterTroughTemperatureCommand ToCommandFromResource(Guid ranchId, Guid waterTroughId, ConfigureWaterTroughTemperatureResource resource) =>
        new(ranchId, waterTroughId, resource.MinimumCelsius, resource.MaximumCelsius, resource.ExpectedVersion);
}
