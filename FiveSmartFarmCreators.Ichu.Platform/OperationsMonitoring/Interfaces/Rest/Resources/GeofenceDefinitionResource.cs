using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
/// A geofence boundary of at least three vertices in decimal degrees. The tolerance is the maximum declared GPS
/// accuracy, in meters, for a position to be conclusive. RanchId and LotId are used only when creating it.
/// </summary>
public sealed record GeofenceDefinitionResource(
    Guid RanchId,
    Guid LotId,
    string? Name,
    int? ToleranceMeters,
    string? Severity,
    IReadOnlyList<CoordinateResource>? Vertices)
{
    public DefineGeofenceCommand ToDefineCommand()
    {
        var (name, tolerance, severity, boundary) = Parse();
        return new DefineGeofenceCommand(RanchId, LotId, name, tolerance, severity, boundary);
    }

    public RedefineGeofenceCommand ToRedefineCommand(Guid geofenceId)
    {
        var (name, tolerance, severity, boundary) = Parse();
        return new RedefineGeofenceCommand(geofenceId, name, tolerance, severity, boundary);
    }

    private (string Name, int Tolerance, AlertSeverity Severity, IReadOnlyList<GeoCoordinate> Boundary) Parse()
    {
        var tolerance = ToleranceMeters ?? throw MonitoringException.InvalidInput("The tolerance in meters is required.");
        var boundary = (Vertices ?? []).Select(v => v.ToCoordinate()).ToList();
        return (Name ?? string.Empty, tolerance, MonitoringValueParser.ParseEnum<AlertSeverity>(Severity, "severity"), boundary);
    }
}
