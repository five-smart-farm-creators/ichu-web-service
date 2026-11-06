namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Response resource for a planned activity.
/// </summary>
/// <param name="Id">The activity identifier.</param>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CampaignId">The optional campaign identifier.</param>
/// <param name="CattleId">The optional animal identifier.</param>
/// <param name="Title">The activity title.</param>
/// <param name="ScheduledAt">The scheduled UTC instant.</param>
public sealed record PlanningActivityResource(
    Guid Id,
    Guid RanchId,
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
