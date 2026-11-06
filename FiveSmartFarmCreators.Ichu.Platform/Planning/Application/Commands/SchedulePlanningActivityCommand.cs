namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;

/// <summary>
/// Requests scheduling an activity for a ranch, campaign, or animal.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CampaignId">An optional campaign identifier.</param>
/// <param name="CattleId">An optional animal identifier.</param>
/// <param name="Title">The activity title.</param>
/// <param name="ScheduledAt">The scheduled UTC instant.</param>
public sealed record SchedulePlanningActivityCommand(
    Guid RanchId,
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
