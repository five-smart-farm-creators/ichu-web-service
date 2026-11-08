using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;

public interface IExecutiveReportQueryService
{
    Task<Result<ExecutiveReport>> HandleAsync(GetExecutiveReportQuery query, CancellationToken cancellationToken = default);
}
