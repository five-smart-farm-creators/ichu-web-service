using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.CommandServices;

/// <summary>
/// Creates campaigns after checking that the caller may manage the ranch plan. A rejected campaign saves nothing.
/// </summary>
public sealed class PlanningCampaignCommandService(
    IPlanningCampaignRepository campaigns,
    IUnitOfWork unitOfWork,
    PlanningGuard guard) : IPlanningCampaignCommandService
{
    /// <inheritdoc />
    public async Task<Result<PlanningCampaign>> HandleAsync(CreatePlanningCampaignCommand command, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(command.RanchId, PlanningPermission.ManagePlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PlanningCampaign>.Failure(access.DomainError);
        }

        var campaign = PlanningCampaign.Create(command.RanchId, command.Name, command.Kind, command.StartsOn, command.EndsOn);
        if (campaign.IsFailure)
        {
            return campaign;
        }

        await campaigns.AddAsync(campaign.Value!, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return campaign;
    }
}
