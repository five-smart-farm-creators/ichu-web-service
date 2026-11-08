using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Creates or replaces the threshold of a ranch for one life stage and metric.
/// </summary>
public record ConfigureThresholdCommand(
    Guid RanchId,
    string LifeStage,
    MonitoredMetric Metric,
    ThresholdRange Range,
    AlertSeverity Severity);
