namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record ThresholdResource(
    Guid ThresholdId,
    Guid RanchId,
    string LifeStage,
    string Metric,
    double MinValue,
    double MaxValue,
    string Severity,
    long Revision);
