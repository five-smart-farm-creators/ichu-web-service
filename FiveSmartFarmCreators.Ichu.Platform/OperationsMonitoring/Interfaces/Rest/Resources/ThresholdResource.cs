using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record ThresholdResource(
    Guid ThresholdId,
    Guid RanchId,
    string LifeStage,
    string Metric,
    double MinValue,
    double MaxValue,
    string Severity,
    long Revision)
{
    public static ThresholdResource From(MonitoringThreshold threshold) =>
        new(threshold.Id, threshold.RanchId, threshold.LifeStage, threshold.Metric.ToString(), threshold.Range.MinValue,
            threshold.Range.MaxValue, threshold.Severity.ToString(), threshold.Revision);
}
