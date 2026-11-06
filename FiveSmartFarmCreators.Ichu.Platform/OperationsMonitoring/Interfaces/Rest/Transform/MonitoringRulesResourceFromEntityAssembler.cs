using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from <see cref="MonitoringRuleSnapshot"/> value objects.
/// </summary>
public static class MonitoringRulesResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms a <see cref="MonitoringRuleSnapshot"/> into a <see cref="MonitoringRulesResource"/>.
    /// </summary>
    /// <param name="snapshot">The rules of a ranch at one revision.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static MonitoringRulesResource ToResourceFromEntity(MonitoringRuleSnapshot snapshot) =>
        new(
            snapshot.RanchId,
            snapshot.Revision,
            snapshot.Thresholds.Select(ThresholdResourceFromEntityAssembler.ToResourceFromEntity).ToList(),
            snapshot.Geofences.Select(GeofenceResourceFromEntityAssembler.ToResourceFromEntity).ToList());
}
