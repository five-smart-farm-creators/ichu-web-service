using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// An alert evaluated by the Edge Gateway. The first reading is the triggering reading; the alert time is its
/// capture time. Threshold alerts carry the evaluated range and geofence alerts the distance to the boundary.
/// </summary>
public sealed record EdgeAlertResource(
    Guid AlertId,
    Guid DeviceId,
    string? Type,
    string? Severity,
    IReadOnlyList<Guid>? ReadingIds,
    long ConfigurationRevision,
    double? ThresholdMinValue,
    double? ThresholdMaxValue,
    Guid? GeofenceId,
    double? DistanceToBoundaryMeters)
{
    public EdgeAlertEntry ToEntry() =>
        new(AlertId, DeviceId, Type, Severity, ReadingIds ?? [], ConfigurationRevision,
            ThresholdMinValue, ThresholdMaxValue, GeofenceId, DistanceToBoundaryMeters);
}
