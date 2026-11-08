using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

/// <summary>
///     Desired water temperature band in degrees Celsius, with one decimal of precision.
/// </summary>
public sealed record TemperatureRange
{
    /// <summary>
    ///     Rebuilds a band that was already validated when it was stored. Requests must use <see cref="Create"/>.
    /// </summary>
    public TemperatureRange(decimal minimumCelsius, decimal maximumCelsius)
    {
        if (!IsValid(minimumCelsius, maximumCelsius))
        {
            throw new ArgumentException("The temperature range must be ordered and use one decimal of precision.", nameof(minimumCelsius));
        }

        MinimumCelsius = minimumCelsius;
        MaximumCelsius = maximumCelsius;
    }

    public decimal MinimumCelsius { get; }

    public decimal MaximumCelsius { get; }

    /// <summary>
    ///     Validates a band from a request, so that an invalid band becomes a business error.
    /// </summary>
    public static Result<TemperatureRange> Create(decimal minimumCelsius, decimal maximumCelsius) =>
        IsValid(minimumCelsius, maximumCelsius)
            ? Result<TemperatureRange>.Success(new TemperatureRange(minimumCelsius, maximumCelsius))
            : Result<TemperatureRange>.Failure(IoTAssetsErrors.InvalidInput);

    private static bool IsValid(decimal minimumCelsius, decimal maximumCelsius) =>
        minimumCelsius < maximumCelsius
        && minimumCelsius >= -999.9m
        && maximumCelsius <= 999.9m
        && decimal.Round(minimumCelsius, 1) == minimumCelsius
        && decimal.Round(maximumCelsius, 1) == maximumCelsius;
}
