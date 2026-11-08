namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The first reading is the triggering reading; the alert time is its capture time. Threshold alerts carry the evaluated range and geofence alerts the distance to the boundary.
/// </summary>
public record EdgeAlertResource(
    Guid AlertId,
    Guid DeviceId,
    string? Type,
    string? Severity,
    IReadOnlyList<Guid>? ReadingIds,
    long ConfigurationRevision,
    double? ThresholdMinValue,
    double? ThresholdMaxValue,
    Guid? GeofenceId,
    double? DistanceToBoundaryMeters);
