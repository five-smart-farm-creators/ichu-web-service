using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Transform;

/// <summary>
///     Maps the campaign request body to its command. The domain validates the values.
/// </summary>
public static class CreatePlanningCampaignCommandFromResourceAssembler
{
    public static CreatePlanningCampaignCommand ToCommandFromResource(CreatePlanningCampaignResource resource) =>
        new(resource.RanchId, resource.Name, resource.Kind, resource.StartsOn, resource.EndsOn);
}
