namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes a lot metric trend, equally weighted animal averages, and reading coverage.
/// </summary>
public sealed record TrendSeries(Guid RanchId, Guid LotId, DateRange Period, DateTimeOffset UpdatedAt, string Source,
    string Metric, string Unit, double? LotAverage, double OutlierDeviation, IReadOnlyList<TrendPoint> Points,
    IReadOnlyList<Guid> OutlierAnimals, int? MissingReadings, string? CoverageReason, string? UnavailableReason);
