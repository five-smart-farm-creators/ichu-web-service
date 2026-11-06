using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="IGeofenceQueryService"/> to read the geofences of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="geofences">The geofence repository.</param>
public sealed class GeofenceQueryService(
    IMonitoringAccessControl accessControl,
    IGeofenceRepository geofences) : IGeofenceQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Geofence>, EGetGeofencesError>> Handle(GetGeofencesByRanchQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<IReadOnlyList<Geofence>, EGetGeofencesError>.Failure(EGetGeofencesError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return new Result<IReadOnlyList<Geofence>, EGetGeofencesError>.Failure(EGetGeofencesError.Forbidden);
        }

        var found = await geofences.FindByRanchAsync(query.RanchId, cancellationToken);
        return new Result<IReadOnlyList<Geofence>, EGetGeofencesError>.Success(found);
    }
}
