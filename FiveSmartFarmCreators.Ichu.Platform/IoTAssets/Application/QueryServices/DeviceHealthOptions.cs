namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;

public sealed class DeviceHealthOptions
{
    private int? _lowBatteryThreshold;

    public int? LowBatteryThreshold
    {
        get => _lowBatteryThreshold;
        set
        {
            if (value is < 0 or > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The battery threshold must be between 0 and 100.");
            }

            _lowBatteryThreshold = value;
        }
    }
}
