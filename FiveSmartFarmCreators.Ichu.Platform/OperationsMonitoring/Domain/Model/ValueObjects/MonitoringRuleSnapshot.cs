using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing a consistent view of a ranch's thresholds and geofences at one revision.
/// </summary>
/// <param name="RanchId">The ranch that owns the rules.</param>
/// <param name="Revision">The rule revision the view corresponds to.</param>
/// <param name="Thresholds">The thresholds configured at that revision.</param>
/// <param name="Geofences">The geofences defined at that revision.</param>
public sealed record MonitoringRuleSnapshot(
    Guid RanchId,
    long Revision,
    IReadOnlyList<MonitoringThreshold> Thresholds,
    IReadOnlyList<Geofence> Geofences);
