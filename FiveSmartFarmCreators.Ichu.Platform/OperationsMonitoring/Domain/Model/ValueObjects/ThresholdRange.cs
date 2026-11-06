namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing the configured normal range of a metric. Values outside it exceed the threshold.
/// </summary>
public sealed record ThresholdRange
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ThresholdRange"/> value object.
    /// </summary>
    /// <param name="minValue">The lowest value considered normal.</param>
    /// <param name="maxValue">The highest value considered normal.</param>
    /// <exception cref="ArgumentException">Thrown when a bound is not finite or the minimum is not below the maximum.</exception>
    public ThresholdRange(double minValue, double maxValue)
    {
        if (!double.IsFinite(minValue) || !double.IsFinite(maxValue) || minValue >= maxValue)
        {
            throw new ArgumentException("A threshold requires finite values with the minimum below the maximum.");
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }

    /// <summary>Gets the lowest value considered normal.</summary>
    public double MinValue { get; }

    /// <summary>Gets the highest value considered normal.</summary>
    public double MaxValue { get; }

    /// <summary>
    ///     Determines whether a value falls outside the range.
    /// </summary>
    /// <param name="value">The captured value.</param>
    /// <returns><see langword="true"/> when the value is below the minimum or above the maximum.</returns>
    public bool IsExceededBy(double value) => value < MinValue || value > MaxValue;
}
