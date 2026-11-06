using System.Text.Json.Serialization;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Rest.Resources;

public sealed record ConfigureWaterTroughTemperatureResource(
    [property: JsonRequired] decimal MinimumCelsius,
    [property: JsonRequired] decimal MaximumCelsius,
    Guid ExpectedVersion);
