using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record ConfigureThresholdCommand(
    Guid RanchId,
    string LifeStage,
    MonitoredMetric Metric,
    ThresholdRange Range,
    AlertSeverity Severity);
