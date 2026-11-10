using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Reads the alerts of a ranch for accounts that may read its monitoring.
/// </summary>
public class MonitoringAlertQueryService(
    IMonitoringAccessControl accessControl,
    IMonitoringAlertRepository alerts,
    IOptions<OperationsMonitoringOptions> options) : IMonitoringAlertQueryService
{
    /// <inheritdoc />
    public async Task<Result<PagedResult<MonitoringAlert>>> Handle(GetAlertsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > options.Value.MaximumPageSize)
        {
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.InvalidPage);
        }

        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        var skip = (query.Page - 1) * query.PageSize;
        var items = await alerts.FindByRanchAsync(query.RanchId, query.Status, query.AnimalId, skip, query.PageSize, cancellationToken);
        var total = await alerts.CountByRanchAsync(query.RanchId, query.Status, query.AnimalId, cancellationToken);
        return Result<PagedResult<MonitoringAlert>>.Success(new PagedResult<MonitoringAlert>(items, query.Page, query.PageSize, total));
    }

    /// <inheritdoc />
    public async Task<Result<MonitoringAlert>> Handle(GetAlertByIdQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        // The ranch is checked before the lookup, and an alert of another ranch is reported as missing, so identifiers cannot be probed.
        var alert = await alerts.FindByIdAsNoTrackingAsync(query.AlertId, cancellationToken);
        if (alert is null || alert.RanchId != query.RanchId)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertNotFound);
        }

        return Result<MonitoringAlert>.Success(alert);
    }
}
