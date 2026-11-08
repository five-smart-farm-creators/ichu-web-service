using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record CreatePlanningCampaignResource(
    Guid RanchId,
    string Name,
    CampaignKind Kind,
    DateOnly StartsOn,
    DateOnly EndsOn);
