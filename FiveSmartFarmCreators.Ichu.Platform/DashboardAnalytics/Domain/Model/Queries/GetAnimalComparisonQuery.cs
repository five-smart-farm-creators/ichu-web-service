using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

/// <summary>
///     Requests an animal comparison against the selected lot metric trend.
/// </summary>
public sealed record GetAnimalComparisonQuery(
    Guid RanchId, Guid LotId, Guid AnimalId, DateRange Period, string Metric, double OutlierDeviation);
