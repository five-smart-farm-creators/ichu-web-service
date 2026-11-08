using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;

/// <summary>
/// Coordinates campaign changes after checking the caller's permission on the ranch.
/// </summary>
public interface IPlanningCampaignCommandService
{
    Task<Result<PlanningCampaign>> HandleAsync(CreatePlanningCampaignCommand command, CancellationToken cancellationToken);
}
