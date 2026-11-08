using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Battery charge reported by a device, as a whole percentage.
/// </summary>
public sealed record BatteryLevel
{
    /// <summary>
    ///     Rebuilds a level that was already validated when it was stored. Requests must use <see cref="Create"/>.
    /// </summary>
    public BatteryLevel(int percentage)
    {
        if (!IsValid(percentage))
        {
            throw new ArgumentOutOfRangeException(nameof(percentage), "A battery level must be between 0 and 100.");
        }

        Percentage = percentage;
    }

    public int Percentage { get; }

    /// <summary>
    ///     Validates a level from a device report, so that an invalid value becomes a business error.
    /// </summary>
    public static Result<BatteryLevel> Create(int percentage) =>
        IsValid(percentage)
            ? Result<BatteryLevel>.Success(new BatteryLevel(percentage))
            : Result<BatteryLevel>.Failure(IoTAssetsErrors.InvalidInput);

    public bool IsLow(int threshold) => Percentage < threshold;

    private static bool IsValid(int percentage) => percentage is >= 0 and <= 100;
}
