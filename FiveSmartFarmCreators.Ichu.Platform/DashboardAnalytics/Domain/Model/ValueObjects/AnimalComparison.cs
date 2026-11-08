namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record AnimalComparison(Guid AnimalId, TrendSeries Reference, double? AnimalAverage,
    double? Deviation, int ReferenceAnimalCount, string? UnavailableReason);
