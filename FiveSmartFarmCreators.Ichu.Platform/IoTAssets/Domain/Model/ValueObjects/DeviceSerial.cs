using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

public sealed record DeviceSerial
{
    public const int MaximumLength = 50;

    public DeviceSerial(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > MaximumLength)
        {
            throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
        }

        Value = value.Trim();
    }

    public string Value { get; }
}
