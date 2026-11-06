using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

public sealed record BatteryLevel
{
    public BatteryLevel(int percentage)
    {
        if (percentage is < 0 or > 100)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        Percentage = percentage;
    }

    public int Percentage { get; }

    public bool IsLow(int threshold)
    {
        if (threshold is < 0 or > 100)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        return Percentage < threshold;
    }
}
