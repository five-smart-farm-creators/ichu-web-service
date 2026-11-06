using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="IMonitoringAlertQueryService"/> to read the alerts of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="alerts">The monitoring alert repository.</param>
/// <param name="options">The technical limits of the module.</param>
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
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.InvalidPage, OperationsMonitoringError.InvalidPage);
        }

        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return Result<PagedResult<MonitoringAlert>>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
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
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Unauthenticated, OperationsMonitoringError.Unauthenticated);
        }

        var alert = await alerts.FindByIdAsync(query.AlertId, cancellationToken);
        if (alert is null)
        {
            return Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.AlertNotFound, OperationsMonitoringError.AlertNotFound);
        }

        return await accessControl.CanAccessRanchAsync(accountId, alert.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken)
            ? Result<MonitoringAlert>.Success(alert)
            : Result<MonitoringAlert>.Failure(OperationsMonitoringErrors.Forbidden, OperationsMonitoringError.Forbidden);
    }
}
