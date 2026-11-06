using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Services;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Application command service that implements <see cref="IMonitoringThresholdCommandService"/> to configure the
///     thresholds of a ranch.
/// </summary>
/// <param name="accessControl">The authorization port implemented with the IAM context.</param>
/// <param name="thresholds">The monitoring threshold repository.</param>
/// <param name="ruleRevisionService">The domain service that advances the rule revision.</param>
/// <param name="unitOfWork">The unit of work used to commit changes.</param>
/// <param name="logger">Logger for diagnostics and error reporting.</param>
public class MonitoringThresholdCommandService(
    IMonitoringAccessControl accessControl,
    IMonitoringThresholdRepository thresholds,
    IMonitoringRuleRevisionService ruleRevisionService,
    IUnitOfWork unitOfWork,
    ILogger<MonitoringThresholdCommandService> logger) : IMonitoringThresholdCommandService
{
    /// <inheritdoc />
    public async Task<Result<MonitoringThreshold, EConfigureThresholdError>> Handle(ConfigureThresholdCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return new Result<MonitoringThreshold, EConfigureThresholdError>.Failure(EConfigureThresholdError.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return new Result<MonitoringThreshold, EConfigureThresholdError>.Failure(EConfigureThresholdError.Forbidden);
        }

        var lifeStage = MonitoringThreshold.NormalizeLifeStage(command.LifeStage);
        var revision = await ruleRevisionService.AdvanceAsync(command.RanchId, cancellationToken);

        var threshold = await thresholds.FindByRanchLifeStageAndMetricAsync(command.RanchId, lifeStage, command.Metric, cancellationToken);
        if (threshold is null)
        {
            threshold = new MonitoringThreshold(command.RanchId, lifeStage, command.Metric, command.Range, command.Severity, revision);
            await thresholds.AddAsync(threshold, cancellationToken);
        }
        else
        {
            threshold.Reconfigure(command.Range, command.Severity, revision);
        }

        try
        {
            await unitOfWork.CompleteAsync(cancellationToken);
            return new Result<MonitoringThreshold, EConfigureThresholdError>.Success(threshold);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "The changes could not be saved.");
            return new Result<MonitoringThreshold, EConfigureThresholdError>.Failure(EConfigureThresholdError.ConcurrentModification);
        }
    }
}
