namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing a threshold configured by a ranch.
/// </summary>
/// <param name="ThresholdId">The threshold identifier.</param>
/// <param name="RanchId">The ranch that configured the threshold.</param>
/// <param name="LifeStage">The production stage the threshold applies to.</param>
/// <param name="Metric">The monitored metric.</param>
/// <param name="MinValue">The lowest value considered normal.</param>
/// <param name="MaxValue">The highest value considered normal.</param>
/// <param name="Severity">The severity of the alerts raised when the range is exceeded.</param>
/// <param name="Revision">The rule revision that last changed the threshold.</param>
public record ThresholdResource(
    Guid ThresholdId,
    Guid RanchId,
    string LifeStage,
    string Metric,
    double MinValue,
    double MaxValue,
    string Severity,
    long Revision);
