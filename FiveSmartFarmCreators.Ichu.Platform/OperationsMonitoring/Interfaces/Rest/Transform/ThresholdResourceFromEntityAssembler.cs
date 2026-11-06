using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="MonitoringThreshold"/> aggregates.
/// </summary>
public static class ThresholdResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="MonitoringThreshold"/> into a <see cref="ThresholdResource"/>.
    /// </summary>
    /// <param name="entity">The threshold aggregate root.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static ThresholdResource ToResourceFromEntity(MonitoringThreshold entity) =>
        new(
            entity.Id,
            entity.RanchId,
            entity.LifeStage,
            entity.Metric.ToString(),
            entity.Range.MinValue,
            entity.Range.MaxValue,
            entity.Severity.ToString(),
            entity.Revision);
}
