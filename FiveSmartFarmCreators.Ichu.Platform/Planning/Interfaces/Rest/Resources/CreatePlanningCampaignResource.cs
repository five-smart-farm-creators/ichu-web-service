using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Enums;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Request body for creating a planning campaign.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="Name">The campaign name.</param>
/// <param name="Kind">The campaign kind: Sanitary or Reproductive.</param>
/// <param name="StartsOn">The first inclusive campaign date in ISO 8601 date format.</param>
/// <param name="EndsOn">The last inclusive campaign date in ISO 8601 date format.</param>
public sealed record CreatePlanningCampaignResource(
    Guid RanchId,
    string Name,
    CampaignKind Kind,
    DateOnly StartsOn,
    DateOnly EndsOn);
