namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record MonitoringRulesResource(
    Guid RanchId,
    long Revision,
    IReadOnlyList<ThresholdResource> Thresholds,
    IReadOnlyList<GeofenceResource> Geofences);
