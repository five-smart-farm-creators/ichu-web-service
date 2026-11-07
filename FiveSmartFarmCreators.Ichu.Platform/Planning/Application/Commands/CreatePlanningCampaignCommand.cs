using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Enums;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;

/// <summary>
/// Requests creation of a sanitary or reproductive planning campaign.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="Name">The campaign name.</param>
/// <param name="Kind">The campaign purpose.</param>
/// <param name="StartsOn">The first inclusive campaign date.</param>
/// <param name="EndsOn">The last inclusive campaign date.</param>
public sealed record CreatePlanningCampaignCommand(
    Guid RanchId,
    string Name,
    CampaignKind Kind,
    DateOnly StartsOn,
    DateOnly EndsOn);
