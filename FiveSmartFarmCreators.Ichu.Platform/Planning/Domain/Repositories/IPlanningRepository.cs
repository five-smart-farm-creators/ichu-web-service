using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Entities;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;

/// <summary>
/// Defines Planning persistence operations without exposing another context's tables.
/// </summary>
public interface IPlanningRepository
{
    /// <summary>
    /// Stores a campaign.
    /// </summary>
    /// <param name="campaign">The campaign to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddCampaignAsync(PlanningCampaign campaign, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a campaign by identifier.
    /// </summary>
    /// <param name="campaignId">The campaign identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The campaign, or null when it does not exist.</returns>
    Task<PlanningCampaign?> FindCampaignAsync(Guid campaignId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists campaigns belonging to a ranch.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ranch campaigns.</returns>
    Task<IReadOnlyList<PlanningCampaign>> FindCampaignsAsync(Guid ranchId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a planned activity.
    /// </summary>
    /// <param name="activity">The activity to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddActivityAsync(PlannedActivity activity, CancellationToken cancellationToken);

    /// <summary>
    /// Finds a planned activity by identifier.
    /// </summary>
    /// <param name="activityId">The activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity, or null when it does not exist.</returns>
    Task<PlannedActivity?> FindActivityAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists activities scheduled in an inclusive UTC range for a ranch.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="from">The first inclusive UTC instant.</param>
    /// <param name="to">The last inclusive UTC instant.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching activities.</returns>
    Task<IReadOnlyList<PlannedActivity>> FindActivitiesAsync(
        Guid ranchId,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stores an application record.
    /// </summary>
    /// <param name="application">The application record to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddApplicationAsync(ApplicationRecord application, CancellationToken cancellationToken);

    /// <summary>
    /// Lists application records associated with a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity application records.</returns>
    Task<IReadOnlyList<ApplicationRecord>> FindApplicationsAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a follow-up.
    /// </summary>
    /// <param name="followUp">The follow-up to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddFollowUpAsync(FollowUp followUp, CancellationToken cancellationToken);

    /// <summary>
    /// Lists follow-ups associated with a planned activity.
    /// </summary>
    /// <param name="activityId">The planned activity identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activity follow-ups.</returns>
    Task<IReadOnlyList<FollowUp>> FindFollowUpsAsync(Guid activityId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores an externally determined withdrawal period.
    /// </summary>
    /// <param name="withdrawalPeriod">The withdrawal period to store.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddWithdrawalPeriodAsync(WithdrawalPeriod withdrawalPeriod, CancellationToken cancellationToken);

    /// <summary>
    /// Lists withdrawal periods for a ranch, optionally filtered by animal.
    /// </summary>
    /// <param name="ranchId">The ranch identifier.</param>
    /// <param name="cattleId">An optional animal identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching withdrawal periods.</returns>
    Task<IReadOnlyList<WithdrawalPeriod>> FindWithdrawalPeriodsAsync(
        Guid ranchId,
        Guid? cattleId,
        CancellationToken cancellationToken);
}
