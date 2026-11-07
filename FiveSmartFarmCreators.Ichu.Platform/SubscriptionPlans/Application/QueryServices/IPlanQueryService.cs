using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.QueryServices;

public interface IPlanQueryService
{
    Task<IReadOnlyList<Plan>> Handle(GetAllPlansQuery query, CancellationToken cancellationToken);

    Task<Result<Plan>> Handle(GetPlanByIdQuery query, CancellationToken cancellationToken);

    Task<Result<PlanQuote>> Handle(EstimatePlanCostQuery query, CancellationToken cancellationToken);
}
