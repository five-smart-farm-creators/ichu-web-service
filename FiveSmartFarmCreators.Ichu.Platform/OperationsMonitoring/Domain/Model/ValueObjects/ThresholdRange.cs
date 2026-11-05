namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>The configured normal range of a metric; values outside it exceed the threshold.</summary>
public sealed record ThresholdRange
{
    public double MinValue { get; }
    public double MaxValue { get; }

    public ThresholdRange(double minValue, double maxValue)
    {
        if (!double.IsFinite(minValue) || !double.IsFinite(maxValue) || minValue >= maxValue)
        {
            throw MonitoringException.InvalidInput("A threshold requires finite values with the minimum below the maximum.");
        }

        MinValue = minValue;
        MaxValue = maxValue;
    }

    public bool IsExceededBy(double value) => value < MinValue || value > MaxValue;
}
