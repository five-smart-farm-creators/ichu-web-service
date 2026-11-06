using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.QueryServices;

/// <summary>
///     Application query service that implements <see cref="IMonitoringAlertQueryService"/> to read the alerts of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="alerts">The monitoring alert repository.</param>
/// <param name="options">The technical limits of the module.</param>
public sealed class MonitoringAlertQueryService(
    IMonitoringAccessControl accessControl,
    IMonitoringAlertRepository alerts,
    IOptions<OperationsMonitoringOptions> options) : IMonitoringAlertQueryService
{
    /// <inheritdoc />
    public async Task<Result<PagedResult<MonitoringAlert>, EGetAlertsError>> Handle(GetAlertsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize < 1 || query.PageSize > options.Value.MaximumPageSize)
        {
            return new Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Failure(EGetAlertsError.InvalidPage);
        }

        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Failure(EGetAlertsError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken))
        {
            return new Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Failure(EGetAlertsError.Forbidden);
        }

        var skip = (query.Page - 1) * query.PageSize;
        var items = await alerts.FindByRanchAsync(query.RanchId, query.Status, query.AnimalId, skip, query.PageSize, cancellationToken);
        var total = await alerts.CountByRanchAsync(query.RanchId, query.Status, query.AnimalId, cancellationToken);
        return new Result<PagedResult<MonitoringAlert>, EGetAlertsError>.Success(new PagedResult<MonitoringAlert>(items, query.Page, query.PageSize, total));
    }

    /// <inheritdoc />
    public async Task<Result<MonitoringAlert, EGetAlertByIdError>> Handle(GetAlertByIdQuery query, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<MonitoringAlert, EGetAlertByIdError>.Failure(EGetAlertByIdError.Unauthenticated);
        }

        var alert = await alerts.FindByIdAsync(query.AlertId, cancellationToken);
        if (alert is null)
        {
            return new Result<MonitoringAlert, EGetAlertByIdError>.Failure(EGetAlertByIdError.AlertNotFound);
        }

        return await accessControl.CanAccessRanchAsync(accountId, alert.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken)
            ? new Result<MonitoringAlert, EGetAlertByIdError>.Success(alert)
            : new Result<MonitoringAlert, EGetAlertByIdError>.Failure(EGetAlertByIdError.Forbidden);
    }
}
