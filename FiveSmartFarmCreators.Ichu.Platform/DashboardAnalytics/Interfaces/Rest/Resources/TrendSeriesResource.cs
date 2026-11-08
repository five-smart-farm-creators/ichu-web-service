namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record TrendSeriesResource(Guid RanchId, Guid LotId, DateRangeResource Period, DateTimeOffset UpdatedAt, string Source,
    string Metric, string Unit, double? LotAverage, double OutlierDeviation, IReadOnlyList<TrendPointResource> Points,
    IReadOnlyList<Guid> OutlierAnimals, int? MissingReadings, string? CoverageReason, string? UnavailableReason);
