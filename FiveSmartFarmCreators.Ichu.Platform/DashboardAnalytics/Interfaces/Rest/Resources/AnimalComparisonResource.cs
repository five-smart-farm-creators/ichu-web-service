namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for animal comparison.
/// </summary>
public sealed record AnimalComparisonResource(Guid AnimalId, TrendSeriesResource Reference, double? AnimalAverage,
    double? Deviation, int ReferenceAnimalCount, string? UnavailableReason);
