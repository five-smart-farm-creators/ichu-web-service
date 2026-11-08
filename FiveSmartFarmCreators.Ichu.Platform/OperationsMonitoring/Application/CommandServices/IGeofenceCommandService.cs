using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;

public interface IGeofenceCommandService
{
    /// <summary>
    ///     Defines the permitted grazing area of a lot. A lot has at most one geofence.
    /// </summary>
    Task<Result<Geofence>> Handle(DefineGeofenceCommand command, CancellationToken cancellationToken);

    Task<Result<Geofence>> Handle(RedefineGeofenceCommand command, CancellationToken cancellationToken);
}
