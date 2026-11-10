using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the activity request body to its command, typing the optional campaign identifier that Planning owns.
/// </summary>
public static class SchedulePlanningActivityCommandFromResourceAssembler
{
    public static SchedulePlanningActivityCommand ToCommandFromResource(Guid ranchId, SchedulePlanningActivityResource resource)
    {
        PlanningCampaignId? campaignId = resource.CampaignId is { } value ? new PlanningCampaignId(value) : null;
        return new SchedulePlanningActivityCommand(ranchId, campaignId, resource.CattleId, resource.Title, resource.ScheduledAt);
    }
}
