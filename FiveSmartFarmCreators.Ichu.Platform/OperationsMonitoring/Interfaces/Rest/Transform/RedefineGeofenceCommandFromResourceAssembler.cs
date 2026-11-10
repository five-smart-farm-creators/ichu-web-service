using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class RedefineGeofenceCommandFromResourceAssembler
{
    public static Result<RedefineGeofenceCommand> ToCommandFromResource(Guid ranchId, Guid geofenceId, RedefineGeofenceResource resource)
    {
        if (resource.ToleranceMeters is not { } tolerance)
        {
            return Result<RedefineGeofenceCommand>.Failure(OperationsMonitoringErrors.ToleranceRequired);
        }

        if (MonitoringEnumParser.Parse<AlertSeverity>(resource.Severity) is not { } severity)
        {
            return Result<RedefineGeofenceCommand>.Failure(OperationsMonitoringErrors.SeverityUnknown);
        }

        var boundary = GeofenceBoundaryFromResourceAssembler.ToBoundary(resource.Vertices);
        return boundary.IsFailure
            ? Result<RedefineGeofenceCommand>.Failure(boundary.DomainError)
            : Result<RedefineGeofenceCommand>.Success(new RedefineGeofenceCommand(
                ranchId, new GeofenceId(geofenceId), resource.Name ?? string.Empty, tolerance, severity, boundary.Value!));
    }
}
