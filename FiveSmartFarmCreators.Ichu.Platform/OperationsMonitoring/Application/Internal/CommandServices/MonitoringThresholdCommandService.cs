using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Configures the thresholds of a ranch. A threshold keeps one identity per life stage and metric, and each change advances the rule revision.
/// </summary>
public class MonitoringThresholdCommandService(
    IMonitoringAccessControl accessControl,
    IMonitoringThresholdRepository thresholds,
    IMonitoringRuleRevisionService ruleRevisionService,
    IOperationsMonitoringUnitOfWork unitOfWork) : IMonitoringThresholdCommandService
{
    /// <inheritdoc />
    public async Task<Result<MonitoringThreshold>> Handle(ConfigureThresholdCommand command, CancellationToken cancellationToken)
    {
        if (await accessControl.GetCurrentAccountIdAsync(cancellationToken) is not { } accountId)
        {
            return Result<MonitoringThreshold>.Failure(OperationsMonitoringErrors.Unauthenticated);
        }

        if (!await accessControl.CanAccessRanchAsync(accountId, command.RanchId, MonitoringPermission.ManageRules, cancellationToken))
        {
            return Result<MonitoringThreshold>.Failure(OperationsMonitoringErrors.Forbidden);
        }

        var lifeStage = MonitoringThreshold.NormalizeLifeStage(command.LifeStage);
        if (lifeStage.IsFailure)
        {
            return Result<MonitoringThreshold>.Failure(lifeStage.DomainError);
        }

        var revision = await ruleRevisionService.AdvanceAsync(command.RanchId, cancellationToken);
        var threshold = await thresholds.FindByRanchLifeStageAndMetricAsync(command.RanchId, lifeStage.Value!, command.Metric, cancellationToken);
        if (threshold is null)
        {
            var creation = MonitoringThreshold.Create(command.RanchId, lifeStage.Value!, command.Metric, command.Range, command.Severity, revision);
            if (creation.IsFailure)
            {
                return creation;
            }

            threshold = creation.Value!;
            await thresholds.AddAsync(threshold, cancellationToken);
        }
        else
        {
            threshold.Reconfigure(command.Range, command.Severity, revision);
        }

        return await unitOfWork.TryCompleteAsync(cancellationToken)
            ? Result<MonitoringThreshold>.Success(threshold)
            : Result<MonitoringThreshold>.Failure(OperationsMonitoringErrors.ConcurrentModification);
    }
}
