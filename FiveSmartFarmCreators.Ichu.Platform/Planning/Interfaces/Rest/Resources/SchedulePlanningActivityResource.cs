using System.Text.Json.Serialization;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record SchedulePlanningActivityResource(
    Guid? CampaignId,
    Guid? CattleId,
    string Title,
    [property: JsonRequired] DateTimeOffset ScheduledAt);
