namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Request body for scheduling a planned activity.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CampaignId">An optional campaign identifier.</param>
/// <param name="CattleId">An optional animal identifier.</param>
/// <param name="Title">The activity title.</param>
/// <param name="ScheduledAt">The scheduled UTC instant in ISO 8601 format.</param>
public sealed record SchedulePlanningActivityResource(
    Guid RanchId,
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
