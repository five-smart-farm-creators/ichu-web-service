using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;

public interface IPlanningCampaignRepository
{
    Task AddAsync(PlanningCampaign campaign, CancellationToken cancellationToken);

    Task<PlanningCampaign?> FindByIdAsNoTrackingAsync(PlanningCampaignId campaignId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PlanningCampaign>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken);
}
