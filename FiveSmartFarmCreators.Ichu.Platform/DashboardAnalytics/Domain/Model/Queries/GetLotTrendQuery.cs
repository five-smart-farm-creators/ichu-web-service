using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

/// <summary>
///     Requests a lot metric trend and an absolute outlier deviation threshold.
/// </summary>
public sealed record GetLotTrendQuery(Guid RanchId, Guid LotId, DateRange Period, string Metric, double OutlierDeviation);
