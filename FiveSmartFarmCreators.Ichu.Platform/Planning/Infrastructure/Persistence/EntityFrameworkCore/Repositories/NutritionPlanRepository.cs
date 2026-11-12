using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
/// Stores nutrition plans in the shared context. Every lookup is filtered by lot, and plans are returned with their ration items.
/// </summary>
public sealed class NutritionPlanRepository(AppDbContext context) : INutritionPlanRepository
{
    /// <inheritdoc />
    public async Task AddAsync(NutritionPlan plan, CancellationToken cancellationToken)
    {
        await context.Set<NutritionPlan>().AddAsync(plan, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NutritionPlan>> FindOverlappingByLotAsync(
        Guid lotId,
        DateOnly validFrom,
        DateOnly? validTo,
        CancellationToken cancellationToken) =>
        await context.Set<NutritionPlan>()
            .Where(plan => plan.LotId == lotId
                && (plan.ValidTo == null || plan.ValidTo >= validFrom)
                && (validTo == null || plan.ValidFrom <= validTo))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<NutritionPlan>> FindByLotAsNoTrackingAsync(Guid lotId, CancellationToken cancellationToken) =>
        await context.Set<NutritionPlan>()
            .AsNoTracking()
            .Where(plan => plan.LotId == lotId)
            .OrderByDescending(plan => plan.ValidFrom)
            .ThenBy(plan => plan.Id)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<NutritionPlan?> FindCurrentByLotAsNoTrackingAsync(Guid lotId, DateOnly on, CancellationToken cancellationToken) =>
        context.Set<NutritionPlan>()
            .AsNoTracking()
            .Where(plan => plan.LotId == lotId && plan.ValidFrom <= on && (plan.ValidTo == null || plan.ValidTo >= on))
            .OrderByDescending(plan => plan.ValidFrom)
            .FirstOrDefaultAsync(cancellationToken);
}
