using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Infrastructure.Persistence;

public sealed class PlanRepository(DbContext context) : IPlanRepository
{
    private IQueryable<Plan> Plans => context.Set<Plan>().AsNoTracking();

    public Task<Plan?> FindAsync(Guid planId, CancellationToken cancellationToken) =>
        Plans.SingleOrDefaultAsync(p => p.Id == planId, cancellationToken);

    public async Task<IReadOnlyList<Plan>> FindPublishedAsync(CancellationToken cancellationToken) =>
        await Plans.Where(p => p.IsPublished).OrderBy(p => p.MinimumHeadCount).ThenBy(p => p.Code).ToListAsync(cancellationToken);
}
