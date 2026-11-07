namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the complete monitoring rules of a ranch at one revision, replacing any earlier
///     revision held by the gateway.
/// </summary>
/// <param name="RanchId">The ranch that owns the rules.</param>
/// <param name="Revision">The rule revision.</param>
/// <param name="Thresholds">The thresholds at that revision.</param>
/// <param name="Geofences">The geofences at that revision.</param>
public record MonitoringRulesResource(
    Guid RanchId,
    long Revision,
    IReadOnlyList<ThresholdResource> Thresholds,
    IReadOnlyList<GeofenceResource> Geofences);
