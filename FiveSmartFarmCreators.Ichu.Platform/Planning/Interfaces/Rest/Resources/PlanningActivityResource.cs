namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record PlanningActivityResource(
    Guid Id,
    Guid RanchId,
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
