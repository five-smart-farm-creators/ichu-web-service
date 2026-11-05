using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public sealed class AlertService(
    MonitoringAuthorization authorization,
    IMonitoringAlertRepository alerts,
    IOperationsMonitoringUnitOfWork unitOfWork,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock)
{
    public async Task<PagedResult<MonitoringAlert>> GetAlertsAsync(GetAlertsQuery query, CancellationToken cancellationToken)
    {
        PagingGuard.Ensure(query.Page, query.PageSize, options.Value.MaximumPageSize);
        await authorization.RequireAsync(query.RanchId, MonitoringPermission.ReadMonitoring, cancellationToken);

        var skip = (query.Page - 1) * query.PageSize;
        var items = await alerts.FindByRanchAsync(query.RanchId, query.Status, query.AnimalId, skip, query.PageSize, cancellationToken);
        var total = await alerts.CountByRanchAsync(query.RanchId, query.Status, query.AnimalId, cancellationToken);
        return new PagedResult<MonitoringAlert>(items, query.Page, query.PageSize, total);
    }

    /// <summary>
    /// Records attention through the central API (TS-09). The response time uses the server time of the request,
    /// which is the event time for online attention; Edge attention keeps its own event time instead.
    /// </summary>
    public async Task<AlertAttentionResult> AcknowledgeAsync(AcknowledgeAlertCommand command, CancellationToken cancellationToken)
    {
        var accountId = await authorization.RequireAccountAsync(cancellationToken);
        var alert = await alerts.FindAsync(command.AlertId, cancellationToken)
            ?? throw MonitoringException.NotFound("The alert does not exist.");
        await authorization.EnsureAsync(accountId, alert.RanchId, MonitoringPermission.AttendAlerts, cancellationToken);

        var now = clock.GetUtcNow();
        var outcome = Acknowledge(alert, command, accountId, now);
        if (outcome != AlertChangeOutcome.Applied)
        {
            return new AlertAttentionResult(outcome, alert);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new AlertAttentionResult(outcome, alert);
        }
        catch (MonitoringException error) when (error.Kind == MonitoringErrorKind.Conflict)
        {
            unitOfWork.DiscardChanges();
            var current = await alerts.FindAsync(command.AlertId, cancellationToken)
                ?? throw MonitoringException.NotFound("The alert does not exist.");

            // Only a replay of the same operation can be confirmed; any other concurrent change is a conflict.
            var replay = current.HasApplied(command.OperationId)
                ? Acknowledge(current, command, accountId, now)
                : AlertChangeOutcome.Conflict;
            return new AlertAttentionResult(replay, current);
        }
    }

    private static AlertChangeOutcome Acknowledge(MonitoringAlert alert, AcknowledgeAlertCommand command, Guid accountId, DateTimeOffset now) =>
        alert.Acknowledge(command.OperationId, accountId, command.Finding, command.Action, now, command.ExpectedVersion, now);
}
