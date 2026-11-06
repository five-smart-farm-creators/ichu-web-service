using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to create or replace the threshold of a ranch for one life stage and metric.
/// </summary>
/// <param name="RanchId">The ranch that configures the threshold.</param>
/// <param name="LifeStage">The production stage the threshold applies to.</param>
/// <param name="Metric">The metric the threshold applies to.</param>
/// <param name="Range">The normal range of the metric.</param>
/// <param name="Severity">The severity of the alerts raised when the range is exceeded.</param>
public sealed record ConfigureThresholdCommand(
    Guid RanchId,
    string LifeStage,
    MonitoredMetric Metric,
    ThresholdRange Range,
    AlertSeverity Severity);
