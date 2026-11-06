using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Enums;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Response resource for a planning campaign.
/// </summary>
/// <param name="Id">The campaign identifier.</param>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="Name">The campaign name.</param>
/// <param name="Kind">The campaign kind.</param>
/// <param name="StartsOn">The first inclusive campaign date.</param>
/// <param name="EndsOn">The last inclusive campaign date.</param>
public sealed record PlanningCampaignResource(
    Guid Id,
    Guid RanchId,
    string Name,
    CampaignKind Kind,
    DateOnly StartsOn,
    DateOnly EndsOn);
