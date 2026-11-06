namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for herd indicator set.
/// </summary>
public sealed record HerdIndicatorSetResource(Guid RanchId, DateRangeResource Period, DateTimeOffset UpdatedAt, string Source,
    IndicatorValueResource MonitoredAnimals, IReadOnlyDictionary<string, int>? OpenAlertsByType,
    string? AlertsUnavailableReason, IndicatorValueResource OutsideGeofenceAnimals, IndicatorValueResource MortalityRate,
    IndicatorValueResource Births, IndicatorValueResource ConfirmedPregnancies, int ExcludedAnimals,
    int? MissingReadings, string? ReadingsCoverageReason);
