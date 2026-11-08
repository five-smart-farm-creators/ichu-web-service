using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Stores planning campaigns in the shared context, filtering every ranch query by its ranch.
/// </summary>
public sealed class PlanningCampaignRepository(AppDbContext context) : IPlanningCampaignRepository
{
    /// <inheritdoc />
    public async Task AddAsync(PlanningCampaign campaign, CancellationToken cancellationToken)
    {
        await context.Set<PlanningCampaign>().AddAsync(campaign, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PlanningCampaign?> FindByIdAsNoTrackingAsync(PlanningCampaignId campaignId, CancellationToken cancellationToken) =>
        context.Set<PlanningCampaign>()
            .AsNoTracking()
            .FirstOrDefaultAsync(campaign => campaign.Id == campaignId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlanningCampaign>> FindByRanchAsNoTrackingAsync(Guid ranchId, CancellationToken cancellationToken) =>
        await context.Set<PlanningCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.RanchId == ranchId)
            .OrderBy(campaign => campaign.StartsOn)
            .ThenBy(campaign => campaign.Id)
            .ToListAsync(cancellationToken);
}
