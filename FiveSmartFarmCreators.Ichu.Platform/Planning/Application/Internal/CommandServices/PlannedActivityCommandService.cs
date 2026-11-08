using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;

/// <summary>
/// Schedules activities, records their applications and schedules their follow-ups. Each change is checked for ranch access and references before the aggregate accepts it.
/// </summary>
public sealed class PlannedActivityCommandService(
    IPlannedActivityRepository activities,
    IPlanningCampaignRepository campaigns,
    IUnitOfWork unitOfWork,
    PlanningGuard guard) : IPlannedActivityCommandService
{
    /// <inheritdoc />
    public async Task<Result<PlannedActivity>> HandleAsync(SchedulePlanningActivityCommand command, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(command.RanchId, PlanningPermission.ManagePlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PlannedActivity>.Failure(access.DomainError);
        }

        if (command.CampaignId is { } campaignId)
        {
            var campaign = await campaigns.FindByIdAsNoTrackingAsync(campaignId, cancellationToken);
            if (campaign is null)
            {
                return Result<PlannedActivity>.Failure(PlanningErrors.CampaignNotFound);
            }

            if (campaign.RanchId != command.RanchId)
            {
                return Result<PlannedActivity>.Failure(PlanningErrors.CampaignRanchMismatch);
            }
        }

        if (command.CattleId is { } cattleId)
        {
            var cattle = await guard.EnsureCattleBelongsToRanchAsync(command.RanchId, cattleId, cancellationToken);
            if (cattle.IsFailure)
            {
                return Result<PlannedActivity>.Failure(cattle.DomainError);
            }
        }

        var activity = PlannedActivity.Create(command.RanchId, command.CampaignId, command.CattleId, command.Title, command.ScheduledAt);
        if (activity.IsFailure)
        {
            return activity;
        }

        await activities.AddAsync(activity.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return activity;
    }

    /// <inheritdoc />
    public async Task<Result<ApplicationRecord>> HandleAsync(RecordApplicationCommand command, CancellationToken cancellationToken)
    {
        var activity = await activities.FindByIdAsync(command.ActivityId, cancellationToken);
        if (activity is null)
        {
            return Result<ApplicationRecord>.Failure(PlanningErrors.ActivityNotFound);
        }

        var access = await guard.EnsureRanchAccessAsync(activity.RanchId, PlanningPermission.RecordClinicalPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<ApplicationRecord>.Failure(access.DomainError);
        }

        var cattle = await guard.EnsureCattleBelongsToRanchAsync(activity.RanchId, command.CattleId, cancellationToken);
        if (cattle.IsFailure)
        {
            return Result<ApplicationRecord>.Failure(cattle.DomainError);
        }

        var intervention = await guard.EnsureInterventionMatchesAsync(
            command.InterventionId, activity.RanchId, command.CattleId, cancellationToken);
        if (intervention.IsFailure)
        {
            return Result<ApplicationRecord>.Failure(intervention.DomainError);
        }

        var application = activity.RecordApplication(command.CattleId, command.AppliedAt, command.InterventionId);
        if (application.IsFailure)
        {
            return application;
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return application;
    }

    /// <inheritdoc />
    public async Task<Result<FollowUp>> HandleAsync(ScheduleFollowUpCommand command, CancellationToken cancellationToken)
    {
        var activity = await activities.FindByIdAsync(command.ActivityId, cancellationToken);
        if (activity is null)
        {
            return Result<FollowUp>.Failure(PlanningErrors.ActivityNotFound);
        }

        var access = await guard.EnsureRanchAccessAsync(activity.RanchId, PlanningPermission.ManagePlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<FollowUp>.Failure(access.DomainError);
        }

        var followUp = activity.ScheduleFollowUp(command.DueAt);
        if (followUp.IsFailure)
        {
            return followUp;
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return followUp;
    }
}
