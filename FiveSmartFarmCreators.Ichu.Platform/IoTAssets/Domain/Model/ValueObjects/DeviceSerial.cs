using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Manufacturer serial of a device, trimmed and unique across the platform.
/// </summary>
public sealed record DeviceSerial
{
    public const int MaximumLength = 50;

    /// <summary>
    ///     Rebuilds a serial that was already validated when it was stored. Requests must use <see cref="Create"/>.
    /// </summary>
    public DeviceSerial(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException("A device serial must contain between 1 and 50 characters.", nameof(value));
        }

        Value = value.Trim();
    }

    public string Value { get; }

    /// <summary>
    ///     Validates a serial from a request, so that an invalid value becomes a business error.
    /// </summary>
    public static Result<DeviceSerial> Create(string value) =>
        IsValid(value)
            ? Result<DeviceSerial>.Success(new DeviceSerial(value))
            : Result<DeviceSerial>.Failure(IoTAssetsErrors.InvalidInput);

    private static bool IsValid(string value) =>
        !string.IsNullOrWhiteSpace(value) && value.Trim().Length <= MaximumLength;
}
