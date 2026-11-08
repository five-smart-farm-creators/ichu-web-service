using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

public sealed record GetLotTrendQuery(Guid RanchId, Guid LotId, DateRange Period, string Metric, double OutlierDeviation);
