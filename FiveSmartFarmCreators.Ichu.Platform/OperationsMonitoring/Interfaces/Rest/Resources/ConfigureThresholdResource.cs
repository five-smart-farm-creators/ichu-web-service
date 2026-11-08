namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record ConfigureThresholdResource(
    Guid RanchId,
    string? LifeStage,
    string? Metric,
    double? MinValue,
    double? MaxValue,
    string? Severity);
