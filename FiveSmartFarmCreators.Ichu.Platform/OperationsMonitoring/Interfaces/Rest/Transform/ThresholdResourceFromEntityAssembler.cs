using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class ThresholdResourceFromEntityAssembler
{
    public static ThresholdResource ToResourceFromEntity(MonitoringThreshold entity) =>
        new(
            entity.Id.Value,
            entity.RanchId,
            entity.LifeStage,
            entity.Metric.ToString(),
            entity.Range.MinValue,
            entity.Range.MaxValue,
            entity.Severity.ToString(),
            entity.Revision);
}
