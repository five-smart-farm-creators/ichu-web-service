namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
/// An alert evaluated by the Edge Gateway. The first reading identifier is the triggering reading; type and
/// severity remain text until the entry is processed so that one malformed entry does not reject the batch.
/// </summary>
public sealed record EdgeAlertEntry(
    Guid AlertId,
    Guid DeviceId,
    string? Type,
    string? Severity,
    IReadOnlyList<Guid> ReadingIds,
    long ConfigurationRevision,
    double? ThresholdMinValue,
    double? ThresholdMaxValue,
    Guid? GeofenceId,
    double? DistanceToBoundaryMeters);
