using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record SchedulePlanningActivityCommand(
    Guid RanchId,
    PlanningCampaignId? CampaignId,
    Guid? CattleId,
    string Title,
    DateTimeOffset ScheduledAt);
