using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;

/// <summary>
/// Reads campaigns after checking the caller's read permission on their ranch.
/// </summary>
public interface IPlanningCampaignQueryService
{
    Task<Result<PlanningCampaign>> HandleAsync(GetPlanningCampaignByIdQuery query, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<PlanningCampaign>>> HandleAsync(GetPlanningCampaignsByRanchQuery query, CancellationToken cancellationToken);
}
