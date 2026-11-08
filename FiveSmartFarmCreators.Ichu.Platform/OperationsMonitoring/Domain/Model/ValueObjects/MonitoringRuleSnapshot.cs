using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     A consistent view of a ranch's thresholds and geofences at one revision, read in one database snapshot.
/// </summary>
public record MonitoringRuleSnapshot(
    Guid RanchId,
    long Revision,
    IReadOnlyList<MonitoringThreshold> Thresholds,
    IReadOnlyList<Geofence> Geofences);
