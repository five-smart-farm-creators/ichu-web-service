namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Identifies a metric that a ranch can configure a threshold for.
/// </summary>
public enum MonitoredMetric
{
    /// <summary>Body temperature in degrees Celsius.</summary>
    BodyTemperature,

    /// <summary>Activity in index units.</summary>
    ActivityIndex
}
