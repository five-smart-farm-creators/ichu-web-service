using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

public sealed record TemperatureRange
{
    public TemperatureRange(decimal minimumCelsius, decimal maximumCelsius)
    {
        if (minimumCelsius >= maximumCelsius
            || minimumCelsius < -999.9m || maximumCelsius > 999.9m
            || decimal.Round(minimumCelsius, 1) != minimumCelsius
            || decimal.Round(maximumCelsius, 1) != maximumCelsius)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        MinimumCelsius = minimumCelsius;
        MaximumCelsius = maximumCelsius;
    }

    public decimal MinimumCelsius { get; }
    public decimal MaximumCelsius { get; }
}
