using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     An alert evaluated by an Edge Gateway. The first reading is the triggering reading. Type and severity are null when the gateway sent a missing or unknown value, so that one malformed entry does not reject the batch.
/// </summary>
public record EdgeAlertEntry(
    MonitoringAlertId AlertId,
    Guid DeviceId,
    AlertType? Type,
    AlertSeverity? Severity,
    IReadOnlyList<Guid> ReadingIds,
    long ConfigurationRevision,
    double? ThresholdMinValue,
    double? ThresholdMaxValue,
    GeofenceId? GeofenceId,
    double? DistanceToBoundaryMeters);
