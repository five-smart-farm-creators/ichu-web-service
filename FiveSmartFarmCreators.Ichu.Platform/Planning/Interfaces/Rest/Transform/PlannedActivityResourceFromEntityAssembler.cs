using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects a planned activity to the REST contract. Its applications and follow-ups are served by their own endpoints.
/// </summary>
public static class PlannedActivityResourceFromEntityAssembler
{
    public static PlanningActivityResource ToResourceFromEntity(PlannedActivity activity) =>
        new(activity.Id.Value, activity.RanchId, activity.CampaignId?.Value, activity.CattleId, activity.Title, activity.ScheduledAt);
}
