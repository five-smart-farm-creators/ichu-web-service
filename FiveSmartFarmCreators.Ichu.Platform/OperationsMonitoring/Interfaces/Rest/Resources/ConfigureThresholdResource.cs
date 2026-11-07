namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the normal range of BodyTemperature (celsius) or ActivityIndex (index) for one life stage.
/// </summary>
/// <param name="RanchId">The ranch that configures the threshold.</param>
/// <param name="LifeStage">The production stage the threshold applies to.</param>
/// <param name="Metric">The metric: BodyTemperature or ActivityIndex.</param>
/// <param name="MinValue">The lowest value considered normal.</param>
/// <param name="MaxValue">The highest value considered normal.</param>
/// <param name="Severity">The severity: Informative, Warning or Critical.</param>
public record ConfigureThresholdResource(
    Guid RanchId,
    string? LifeStage,
    string? Metric,
    double? MinValue,
    double? MaxValue,
    string? Severity);
