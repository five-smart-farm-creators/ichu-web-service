using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.QueryServices;

/// <summary>
///     Public read service of the plan catalog. Quotes come from the plan aggregate, so estimates and real requests
///     follow the same rules.
/// </summary>
public class PlanQueryService(IPlanRepository plans) : IPlanQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken) =>
        await plans.ListPublishedAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<Result<Plan>> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken)
    {
        var plan = await plans.FindByIdAsync(query.PlanId, cancellationToken);
        return plan is null
            ? Result<Plan>.Failure(SubscriptionPlansErrors.PlanNotFound)
            : Result<Plan>.Success(plan);
    }

    /// <inheritdoc />
    public async Task<Result<PlanQuote>> Handle(EstimatePlanCostQuery query, CancellationToken cancellationToken)
    {
        var headCount = HeadCount.Create(query.HeadCount);
        if (headCount.IsFailure)
        {
            return Result<PlanQuote>.Failure(headCount.DomainError);
        }

        var plan = await plans.FindByIdAsync(query.PlanId, cancellationToken);
        if (plan is null)
        {
            return Result<PlanQuote>.Failure(SubscriptionPlansErrors.PlanNotFound);
        }

        return plan.EstimateFor(headCount.Value!);
    }
}
