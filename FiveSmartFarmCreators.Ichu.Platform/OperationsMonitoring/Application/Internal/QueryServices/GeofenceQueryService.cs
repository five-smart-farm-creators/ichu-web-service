using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="IGeofenceQueryService"/> to read the geofences of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="geofences">The geofence repository.</param>
public class GeofenceQueryService(
    IMonitoringAccessControl accessControl,
    IGeofenceRepository geofences) : IGeofenceQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Geofence>>> Handle(GetGeofencesByRanchQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<Geofence>>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<IReadOnlyList<Geofence>>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
        }

        var found = await geofences.FindByRanchAsync(query.RanchId, cancellationToken);
        return Result<IReadOnlyList<Geofence>>.Success(found);
    }
}
