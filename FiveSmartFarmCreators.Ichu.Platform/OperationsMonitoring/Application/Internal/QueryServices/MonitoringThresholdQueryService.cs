using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Reads the thresholds of a ranch for accounts that may read its monitoring.
/// </summary>
public class MonitoringThresholdQueryService(
    IMonitoringAccessControl accessControl,
    IMonitoringThresholdRepository thresholds) : IMonitoringThresholdQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MonitoringThreshold>>> Handle(GetThresholdsByRanchQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<MonitoringThreshold>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<IReadOnlyList<MonitoringThreshold>>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        return Result<IReadOnlyList<MonitoringThreshold>>.Success(await thresholds.FindByRanchAsync(query.RanchId, cancellationToken));
    }
}
