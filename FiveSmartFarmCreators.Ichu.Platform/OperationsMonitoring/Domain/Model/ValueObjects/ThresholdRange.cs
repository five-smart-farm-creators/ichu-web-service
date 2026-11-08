using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     The normal range of a metric. Values outside it exceed the threshold.
/// </summary>
public record ThresholdRange
{
    /// <summary>
    ///     Rebuilds a range that was validated when it was configured. Requests must use <see cref="Create"/>.
    /// </summary>
    public ThresholdRange(double minValue, double maxValue)
    {
        if (!IsValid(minValue, maxValue))
        {
            throw new ArgumentException("A threshold requires finite values with the minimum below the maximum.", nameof(minValue));
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }

    public double MinValue { get; }

    public double MaxValue { get; }

    public static Result<ThresholdRange> Create(double minValue, double maxValue) =>
        IsValid(minValue, maxValue)
            ? Result<ThresholdRange>.Success(new ThresholdRange(minValue, maxValue))
            : Result<ThresholdRange>.Failure(OperationsMonitoringErrors.ThresholdRangeInvalid);

    public bool IsExceededBy(double value) => value < MinValue || value > MaxValue;

    private static bool IsValid(double minValue, double maxValue) =>
        double.IsFinite(minValue) && double.IsFinite(maxValue) && minValue < maxValue;
}
