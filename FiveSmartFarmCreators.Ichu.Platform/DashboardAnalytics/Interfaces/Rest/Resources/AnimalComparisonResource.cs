namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record AnimalComparisonResource(Guid AnimalId, TrendSeriesResource Reference, double? AnimalAverage,
    double? Deviation, int ReferenceAnimalCount, string? UnavailableReason);
