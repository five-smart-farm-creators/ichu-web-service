namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record HerdIndicatorSet(Guid RanchId, DateRange Period, DateTimeOffset UpdatedAt, string Source,
    IndicatorValue MonitoredAnimals, IReadOnlyDictionary<string, int>? OpenAlertsByType,
    string? AlertsUnavailableReason, IndicatorValue OutsideGeofenceAnimals, IndicatorValue MortalityRate,
    IndicatorValue Births, IndicatorValue ConfirmedPregnancies, int ExcludedAnimals, int? MissingReadings, string? ReadingsCoverageReason);
