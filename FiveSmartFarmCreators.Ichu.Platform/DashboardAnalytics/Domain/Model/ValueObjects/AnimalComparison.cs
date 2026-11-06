namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes an animal average relative to its lot and any unavailable comparison reason.
/// </summary>
public sealed record AnimalComparison(Guid AnimalId, TrendSeries Reference, double? AnimalAverage,
    double? Deviation, int ReferenceAnimalCount, string? UnavailableReason);
