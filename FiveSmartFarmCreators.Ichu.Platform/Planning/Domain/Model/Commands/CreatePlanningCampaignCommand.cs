using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record CreatePlanningCampaignCommand(Guid RanchId, string Name, CampaignKind Kind, DateOnly StartsOn, DateOnly EndsOn);
