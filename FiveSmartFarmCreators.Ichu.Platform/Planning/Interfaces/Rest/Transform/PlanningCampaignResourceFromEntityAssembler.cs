using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Projects a campaign to the REST contract without exposing the aggregate.
/// </summary>
public static class PlanningCampaignResourceFromEntityAssembler
{
    public static PlanningCampaignResource ToResourceFromEntity(PlanningCampaign campaign) =>
        new(campaign.Id.Value, campaign.RanchId, campaign.Name, campaign.Kind, campaign.StartsOn, campaign.EndsOn);
}
