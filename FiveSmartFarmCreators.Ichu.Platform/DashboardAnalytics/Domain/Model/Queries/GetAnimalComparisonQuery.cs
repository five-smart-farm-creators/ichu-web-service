using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

public sealed record GetAnimalComparisonQuery(
    Guid RanchId, Guid LotId, Guid AnimalId, DateRange Period, string Metric, double OutlierDeviation);
