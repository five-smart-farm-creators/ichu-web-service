using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;

/// <summary>
///     Application query service interface for handling geofence read operations.
/// </summary>
public interface IGeofenceQueryService
{
    /// <summary>
    ///     Handles the listing of the geofences of an authorized ranch asynchronously.
    /// </summary>
    /// <param name="query">The query containing the ranch identifier.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A result containing the geofences on success, or an error enum on failure.</returns>
    Task<Result<IReadOnlyList<Geofence>, EGetGeofencesError>> Handle(GetGeofencesByRanchQuery query, CancellationToken cancellationToken);
}
