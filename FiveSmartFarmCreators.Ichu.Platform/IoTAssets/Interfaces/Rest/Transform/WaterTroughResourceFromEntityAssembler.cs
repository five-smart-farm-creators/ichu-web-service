using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Transform;

public static class WaterTroughResourceFromEntityAssembler
{
    public static WaterTroughResource ToResourceFromEntity(WaterTrough waterTrough) =>
        new(waterTrough.Id, waterTrough.RanchId, waterTrough.Name, waterTrough.LotId,
            waterTrough.TemperatureRange is null ? null : new TemperatureRangeResource(
                waterTrough.TemperatureRange.MinimumCelsius, waterTrough.TemperatureRange.MaximumCelsius),
            waterTrough.Version);
}
