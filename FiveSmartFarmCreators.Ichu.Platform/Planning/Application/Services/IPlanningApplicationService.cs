using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Services;

/// <summary>
/// Defines Planning use cases exposed to the REST interface.
/// </summary>
public interface IPlanningApplicationService
{
    /// <summary>
    /// Creates a campaign after checking ranch access.
    /// </summary>
    /// <param name="command">The campaign data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created campaign.</returns>
    Task<PlanningCampaign> CreateCampaignAsync(CreatePlanningCampaignCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a campaign after checking access to its ranch.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The campaign.</returns>
    Task<PlanningCampaign> GetCampaignAsync(Guid campaignId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists campaigns after checking ranch access.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The campaigns for the ranch.</returns>
    Task<IReadOnlyList<PlanningCampaign>> GetCampaignsAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules an activity after checking ranch and animal references.
    /// </summary>
    /// <param name="command">The activity data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled activity.</returns>
    Task<PlannedActivity> ScheduleActivityAsync(SchedulePlanningActivityCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an activity after checking access to its ranch.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity.</returns>
    Task<PlannedActivity> GetActivityAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists activities in a calendar range after checking ranch access.
    /// </summary>
    /// <param name="query">The ranch and inclusive UTC interval.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching activities.</returns>
    Task<IReadOnlyList<PlannedActivity>> GetCalendarAsync(GetPlanningCalendarQuery query, CancellationToken cancellationToken);

    /// <summary>
    /// Records an application and optionally links its OperationsMonitoring intervention.
    /// </summary>
    /// <param name="command">The application data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application record.</returns>
    Task<ApplicationRecord> RecordApplicationAsync(RecordApplicationCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Lists application records after checking access to their activity's ranch.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity application records.</returns>
    Task<IReadOnlyList<ApplicationRecord>> GetApplicationsAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules a follow-up after checking access to its activity's ranch.
    /// </summary>
    /// <param name="command">The follow-up data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The scheduled follow-up.</returns>
    Task<FollowUp> ScheduleFollowUpAsync(ScheduleFollowUpCommand command, CancellationToken cancellationToken);

    /// <summary>
    /// Lists follow-ups after checking access to their activity's ranch.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity follow-ups.</returns>
    Task<IReadOnlyList<FollowUp>> GetFollowUpsAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Records an explicit withdrawal interval without calculating clinical duration.
    /// </summary>
    /// <param name="command">The withdrawal interval data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The recorded withdrawal period.</returns>
    Task<WithdrawalPeriod> RecordWithdrawalPeriodAsync(
        RecordWithdrawalPeriodCommand command,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists withdrawal periods after checking ranch and animal references.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cattleId">An optional animal identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching withdrawal periods.</returns>
    Task<IReadOnlyList<WithdrawalPeriod>> GetWithdrawalPeriodsAsync(
        Guid ranchId,
        Guid? cattleId,
        CancellationToken cancellationToken);
}
