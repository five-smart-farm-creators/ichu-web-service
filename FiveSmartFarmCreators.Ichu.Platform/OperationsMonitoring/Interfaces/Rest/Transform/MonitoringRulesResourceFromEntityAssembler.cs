using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class MonitoringRulesResourceFromEntityAssembler
{
    public static MonitoringRulesResource ToResourceFromEntity(MonitoringRuleSnapshot snapshot) =>
        new(
            snapshot.RanchId,
            snapshot.Revision,
            snapshot.Thresholds.Select(ThresholdResourceFromEntityAssembler.ToResourceFromEntity).ToList(),
            snapshot.Geofences.Select(GeofenceResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
