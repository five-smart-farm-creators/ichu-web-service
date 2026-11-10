using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.QueryServices;

/// <summary>
/// Reads campaigns for accounts that may read their ranch. A campaign of another ranch is reported as not found.
/// </summary>
public sealed class PlanningCampaignQueryService(
    IPlanningCampaignRepository campaigns,
    PlanningGuard guard) : IPlanningCampaignQueryService
{
    /// <inheritdoc />
    public async Task<Result<PlanningCampaign>> HandleAsync(GetPlanningCampaignByIdQuery query, CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<PlanningCampaign>.Failure(access.DomainError);
        }

        // The ranch is checked before the lookup, and a campaign of another ranch is reported as missing, so identifiers cannot be probed.
        var campaign = await campaigns.FindByIdAsNoTrackingAsync(query.CampaignId, cancellationToken);
        if (campaign is null || campaign.RanchId != query.RanchId)
        {
            return Result<PlanningCampaign>.Failure(PlanningErrors.CampaignNotFound);
        }

        return Result<PlanningCampaign>.Success(campaign);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PlanningCampaign>>> HandleAsync(
        GetPlanningCampaignsByRanchQuery query,
        CancellationToken cancellationToken)
    {
        var access = await guard.EnsureRanchAccessAsync(query.RanchId, PlanningPermission.ReadPlanning, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<PlanningCampaign>>.Failure(access.DomainError);
        }

        return Result<IReadOnlyList<PlanningCampaign>>.Success(
            await campaigns.FindByRanchAsNoTrackingAsync(query.RanchId, cancellationToken));
    }
}
