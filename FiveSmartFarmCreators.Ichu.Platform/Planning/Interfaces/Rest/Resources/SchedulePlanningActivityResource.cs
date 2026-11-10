namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record SchedulePlanningActivityResource(
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
