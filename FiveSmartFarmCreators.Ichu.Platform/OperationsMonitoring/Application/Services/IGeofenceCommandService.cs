using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;

/// <summary>
///     Application command service interface for handling geofence write operations.
/// </summary>
public interface IGeofenceCommandService
{
    /// <summary>
    ///     Handles the definition of the permitted grazing area of a lot asynchronously. A lot has at most one geofence.
    /// </summary>
    /// <param name="command">The command containing the geofence definition.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the stored geofence on success, or an error enum on failure.</returns>
    /// <exception cref="ArgumentException">Thrown when the geofence definition violates a domain rule.</exception>
    Task<Result<Geofence, EDefineGeofenceError>> Handle(DefineGeofenceCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the redefinition of an existing geofence asynchronously.
    /// </summary>
    /// <param name="command">The command containing the new geofence definition.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the stored geofence on success, or an error enum on failure.</returns>
    /// <exception cref="ArgumentException">Thrown when the geofence definition violates a domain rule.</exception>
    Task<Result<Geofence, ERedefineGeofenceError>> Handle(RedefineGeofenceCommand command, CancellationToken cancellationToken);
}
