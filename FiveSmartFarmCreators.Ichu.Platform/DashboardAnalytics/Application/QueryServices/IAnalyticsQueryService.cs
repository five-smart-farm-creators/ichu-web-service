using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Application.QueryServices;

public interface IAnalyticsQueryService
{
    Task<Result<HerdIndicatorSet>> HandleAsync(GetHerdIndicatorsQuery query, CancellationToken cancellationToken = default);

    Task<Result<TrendSeries>> HandleAsync(GetLotTrendQuery query, CancellationToken cancellationToken = default);

    Task<Result<AnimalComparison>> HandleAsync(GetAnimalComparisonQuery query, CancellationToken cancellationToken = default);

    Task<Result<EpidemiologicalCurve>> HandleAsync(GetEpidemiologicalCurveQuery query, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<RanchPortfolioEntry>>> HandleAsync(GetRanchPortfolioQuery query, CancellationToken cancellationToken = default);
}
