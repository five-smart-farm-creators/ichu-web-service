using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;

/// <summary>
/// Coordinates Planning use cases and verifies every ranch and animal reference.
/// </summary>
public sealed class PlanningApplicationService : IPlanningApplicationService
{
    private readonly IPlanningRepository _planningRepository;
    private readonly ICurrentAccountContext _currentAccountContext;
    private readonly IPlanningAccessPolicy _planningAccessPolicy;
    private readonly ICattleInformationContext _cattleInformationContext;
    private readonly IOperationsMonitoringContext _operationsMonitoringContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningApplicationService"/> class.
    /// </summary>
    /// <param name="planningRepository">Planning persistence operations.</param>
    /// <param name="currentAccountContext">The authenticated Iam account.</param>
    /// <param name="planningAccessPolicy">The Iam ranch access policy.</param>
    /// <param name="cattleInformationContext">The CattleInformation reference adapter.</param>
    /// <param name="operationsMonitoringContext">The OperationsMonitoring reference adapter.</param>
    public PlanningApplicationService(
        IPlanningRepository planningRepository,
        ICurrentAccountContext currentAccountContext,
        IPlanningAccessPolicy planningAccessPolicy,
        ICattleInformationContext cattleInformationContext,
        IOperationsMonitoringContext operationsMonitoringContext)
    {
        _planningRepository = planningRepository;
        _currentAccountContext = currentAccountContext;
        _planningAccessPolicy = planningAccessPolicy;
        _cattleInformationContext = cattleInformationContext;
        _operationsMonitoringContext = operationsMonitoringContext;
    }

    /// <inheritdoc />
    public async Task<PlanningCampaign> CreateCampaignAsync(
        CreatePlanningCampaignCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(command.RanchId, cancellationToken);

        var campaign = PlanningCampaign.Create(
            Guid.NewGuid(),
            command.RanchId,
            command.Name,
            command.Kind,
            command.StartsOn,
            command.EndsOn);

        await _planningRepository.AddCampaignAsync(campaign, cancellationToken);
        return campaign;
    }

    /// <inheritdoc />
    public async Task<PlanningCampaign> GetCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var campaign = await _planningRepository.FindCampaignAsync(campaignId, cancellationToken)
            ?? throw NotFound("planning.campaign_not_found", "The campaign was not found.");

        await EnsureRanchAccessAsync(campaign.RanchId, cancellationToken);
        return campaign;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlanningCampaign>> GetCampaignsAsync(
        Guid ranchId,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(ranchId, cancellationToken);
        return await _planningRepository.FindCampaignsAsync(ranchId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlannedActivity> ScheduleActivityAsync(
        SchedulePlanningActivityCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(command.RanchId, cancellationToken);

        if (command.CampaignId is Guid campaignId)
        {
            var campaign = await _planningRepository.FindCampaignAsync(campaignId, cancellationToken)
                ?? throw NotFound("planning.campaign_not_found", "The campaign was not found.");

            if (campaign.RanchId != command.RanchId)
            {
                throw Conflict("planning.campaign_ranch_mismatch", "The campaign belongs to a different ranch.");
            }
        }

        if (command.CattleId is Guid cattleId)
        {
            await EnsureCattleBelongsToRanchAsync(cattleId, command.RanchId, cancellationToken);
        }

        var activity = PlannedActivity.Create(
            Guid.NewGuid(),
            command.RanchId,
            command.CampaignId,
            command.CattleId,
            command.Title,
            command.ScheduledAt);

        await _planningRepository.AddActivityAsync(activity, cancellationToken);
        return activity;
    }

    /// <inheritdoc />
    public async Task<PlannedActivity> GetActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        var activity = await FindActivityAsync(activityId, cancellationToken);
        await EnsureRanchAccessAsync(activity.RanchId, cancellationToken);
        return activity;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlannedActivity>> GetCalendarAsync(
        GetPlanningCalendarQuery query,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(query.RanchId, cancellationToken);

        if (query.From.Offset != TimeSpan.Zero || query.To.Offset != TimeSpan.Zero)
        {
            throw new PlanningDomainException("planning.utc_timestamp_required", "Calendar range timestamps must use UTC.");
        }

        if (query.To < query.From)
        {
            throw new PlanningDomainException("planning.calendar_range_invalid", "The calendar end must be on or after its start.");
        }

        return await _planningRepository.FindActivitiesAsync(query.RanchId, query.From, query.To, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApplicationRecord> RecordApplicationAsync(
        RecordApplicationCommand command,
        CancellationToken cancellationToken)
    {
        var activity = await FindActivityAsync(command.ActivityId, cancellationToken);
        await EnsureRanchAccessAsync(activity.RanchId, cancellationToken);

        if (activity.CattleId is Guid plannedCattleId && plannedCattleId != command.CattleId)
        {
            throw Conflict("planning.activity_cattle_mismatch", "The animal does not match the planned activity.");
        }

        await EnsureCattleBelongsToRanchAsync(command.CattleId, activity.RanchId, cancellationToken);

        if (command.InterventionId is Guid interventionId)
        {
            var intervention = await _operationsMonitoringContext.FindInterventionAsync(interventionId, cancellationToken)
                ?? throw NotFound("planning.intervention_not_found", "The clinical intervention was not found.");

            if (intervention.CattleId != command.CattleId)
            {
                throw Conflict("planning.intervention_cattle_mismatch", "The intervention belongs to a different animal.");
            }
        }

        var application = ApplicationRecord.Create(
            Guid.NewGuid(),
            command.ActivityId,
            command.CattleId,
            command.AppliedAt,
            command.InterventionId);

        await _planningRepository.AddApplicationAsync(application, cancellationToken);
        return application;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ApplicationRecord>> GetApplicationsAsync(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        var activity = await FindActivityAsync(activityId, cancellationToken);
        await EnsureRanchAccessAsync(activity.RanchId, cancellationToken);
        return await _planningRepository.FindApplicationsAsync(activityId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FollowUp> ScheduleFollowUpAsync(
        ScheduleFollowUpCommand command,
        CancellationToken cancellationToken)
    {
        var activity = await FindActivityAsync(command.ActivityId, cancellationToken);
        await EnsureRanchAccessAsync(activity.RanchId, cancellationToken);

        var followUp = FollowUp.Create(Guid.NewGuid(), command.ActivityId, command.DueAt);
        await _planningRepository.AddFollowUpAsync(followUp, cancellationToken);
        return followUp;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FollowUp>> GetFollowUpsAsync(
        Guid activityId,
        CancellationToken cancellationToken)
    {
        var activity = await FindActivityAsync(activityId, cancellationToken);
        await EnsureRanchAccessAsync(activity.RanchId, cancellationToken);
        return await _planningRepository.FindFollowUpsAsync(activityId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WithdrawalPeriod> RecordWithdrawalPeriodAsync(
        RecordWithdrawalPeriodCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(command.RanchId, cancellationToken);
        await EnsureCattleBelongsToRanchAsync(command.CattleId, command.RanchId, cancellationToken);

        if (command.InterventionId is Guid interventionId)
        {
            var intervention = await _operationsMonitoringContext.FindInterventionAsync(interventionId, cancellationToken)
                ?? throw NotFound("planning.intervention_not_found", "The clinical intervention was not found.");

            if (intervention.CattleId != command.CattleId)
            {
                throw Conflict("planning.intervention_cattle_mismatch", "The intervention belongs to a different animal.");
            }
        }

        var withdrawalPeriod = WithdrawalPeriod.Create(
            Guid.NewGuid(),
            command.RanchId,
            command.CattleId,
            command.InterventionId,
            command.StartsAt,
            command.EndsAt);

        await _planningRepository.AddWithdrawalPeriodAsync(withdrawalPeriod, cancellationToken);
        return withdrawalPeriod;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WithdrawalPeriod>> GetWithdrawalPeriodsAsync(
        Guid ranchId,
        Guid? cattleId,
        CancellationToken cancellationToken)
    {
        await EnsureRanchAccessAsync(ranchId, cancellationToken);

        if (cattleId is Guid requestedCattleId)
        {
            await EnsureCattleBelongsToRanchAsync(requestedCattleId, ranchId, cancellationToken);
        }

        return await _planningRepository.FindWithdrawalPeriodsAsync(ranchId, cattleId, cancellationToken);
    }

    private async Task<PlannedActivity> FindActivityAsync(Guid activityId, CancellationToken cancellationToken)
    {
        return await _planningRepository.FindActivityAsync(activityId, cancellationToken)
            ?? throw NotFound("planning.activity_not_found", "The planned activity was not found.");
    }

    private async Task EnsureRanchAccessAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "A ranch identifier is required.");
        }

        var accountId = _currentAccountContext.GetRequiredAccountId();
        if (accountId == Guid.Empty ||
            !await _planningAccessPolicy.CanManageRanchAsync(accountId, ranchId, cancellationToken))
        {
            throw Forbidden();
        }
    }

    private async Task EnsureCattleBelongsToRanchAsync(
        Guid cattleId,
        Guid ranchId,
        CancellationToken cancellationToken)
    {
        if (cattleId == Guid.Empty)
        {
            throw new PlanningDomainException("planning.invalid_identifier", "An animal identifier is required.");
        }

        var cattle = await _cattleInformationContext.FindCattleAsync(cattleId, cancellationToken)
            ?? throw NotFound("planning.cattle_not_found", "The animal was not found.");

        if (cattle.CattleId != cattleId)
        {
            throw Conflict("planning.cattle_reference_invalid", "CattleInformation returned a mismatched animal reference.");
        }

        if (cattle.RanchId != ranchId)
        {
            throw Forbidden();
        }
    }

    private static PlanningRequestException NotFound(string code, string message)
    {
        return new PlanningRequestException(PlanningFailureKind.NotFound, code, message);
    }

    private static PlanningRequestException Conflict(string code, string message)
    {
        return new PlanningRequestException(PlanningFailureKind.Conflict, code, message);
    }

    private static PlanningRequestException Forbidden()
    {
        return new PlanningRequestException(
            PlanningFailureKind.Forbidden,
            "planning.access_denied",
            "The current account cannot access the requested ranch.");
    }
}
