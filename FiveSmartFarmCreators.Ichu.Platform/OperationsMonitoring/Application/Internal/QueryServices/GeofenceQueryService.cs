using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Reads the geofences of a ranch for accounts that may read its monitoring.
/// </summary>
public class GeofenceQueryService(IMonitoringAccessControl accessControl, IGeofenceRepository geofences) : IGeofenceQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Geofence>>> Handle(GetGeofencesByRanchQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<Geofence>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<IReadOnlyList<Geofence>>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        return Result<IReadOnlyList<Geofence>>.Success(await geofences.FindByRanchAsync(query.RanchId, cancellationToken));
    }
}
