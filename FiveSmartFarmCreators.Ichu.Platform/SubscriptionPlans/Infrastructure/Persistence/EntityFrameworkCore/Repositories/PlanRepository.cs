using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PlanRepository(AppDbContext context) : IPlanRepository
{
    /// <inheritdoc />
    public async Task<Plan?> FindByIdAsync(PlanId id, CancellationToken cancellationToken = default) =>
        await context.Set<Plan>().AsNoTracking().FirstOrDefaultAsync(plan => plan.Id == id, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Plan>> ListPublishedAsync(CancellationToken cancellationToken = default) =>
        await context.Set<Plan>()
            .AsNoTracking()
            .Where(plan => plan.IsPublished)
            .OrderBy(plan => plan.DeviceLimit)
            .ToListAsync(cancellationToken);
}
