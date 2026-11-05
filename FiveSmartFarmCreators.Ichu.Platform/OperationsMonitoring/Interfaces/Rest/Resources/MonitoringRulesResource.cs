using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>The complete monitoring rules of a ranch at one revision, replacing any earlier revision held by the gateway.</summary>
public sealed record MonitoringRulesResource(
    Guid RanchId,
    long Revision,
    IReadOnlyList<ThresholdResource> Thresholds,
    IReadOnlyList<GeofenceResource> Geofences)
{
    public static MonitoringRulesResource From(MonitoringRuleSnapshot snapshot) =>
        new(
            snapshot.RanchId,
            snapshot.Revision,
            snapshot.Thresholds.Select(ThresholdResource.From).ToList(),
            snapshot.Geofences.Select(GeofenceResource.From).ToList());
}
