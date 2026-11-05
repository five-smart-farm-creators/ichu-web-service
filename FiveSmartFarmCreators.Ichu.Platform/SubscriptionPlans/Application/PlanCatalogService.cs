using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

/// <summary>Public plan catalog and cost estimation (US-35, TS-10). Estimates are never charges.</summary>
public sealed class PlanCatalogService(IPlanRepository plans, TimeProvider clock)
{
    public async Task<IReadOnlyList<Plan>> GetOfferedPlansAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        return (await plans.FindPublishedAsync(cancellationToken)).Where(p => p.IsOfferedAt(now)).ToList();
    }

    public async Task<Plan> GetPlanAsync(Guid planId, CancellationToken cancellationToken)
    {
        var plan = await plans.FindAsync(planId, cancellationToken);
        return plan is { IsPublished: true } ? plan : throw SubscriptionException.NotFound("The plan does not exist.");
    }

    public async Task<PlanQuoteResult> QuoteAsync(Guid planId, int headCount, CancellationToken cancellationToken)
    {
        if (headCount < 1)
        {
            throw SubscriptionException.InvalidInput("The head count must be a positive integer.");
        }

        var plan = await GetPlanAsync(planId, cancellationToken);
        var now = clock.GetUtcNow();
        if (plan.Covers(headCount))
        {
            return new PlanQuoteResult(plan.Quote(headCount, now), null, null);
        }

        // The suggestion is the narrowest offered coverage that admits the head count.
        var suggestion = (await GetOfferedPlansAsync(cancellationToken))
            .Where(p => p.Id != plan.Id && p.Covers(headCount))
            .OrderBy(p => p.MaximumHeadCount ?? int.MaxValue)
            .ThenBy(p => p.Code, StringComparer.Ordinal)
            .FirstOrDefault();
        return new PlanQuoteResult(null, "The head count is outside the coverage of the plan.", suggestion?.Id);
    }
}
