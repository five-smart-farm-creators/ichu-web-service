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
///     Application query service that implements <see cref="IMonitoringThresholdQueryService"/> to read the thresholds of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="thresholds">The monitoring threshold repository.</param>
public class MonitoringThresholdQueryService(
    IMonitoringAccessControl accessControl,
    IMonitoringThresholdRepository thresholds) : IMonitoringThresholdQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<MonitoringThreshold>>> Handle(GetThresholdsByRanchQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<IReadOnlyList<MonitoringThreshold>>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<IReadOnlyList<MonitoringThreshold>>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
        }

        var found = await thresholds.FindByRanchAsync(query.RanchId, cancellationToken);
        return Result<IReadOnlyList<MonitoringThreshold>>.Success(found);
    }
}
