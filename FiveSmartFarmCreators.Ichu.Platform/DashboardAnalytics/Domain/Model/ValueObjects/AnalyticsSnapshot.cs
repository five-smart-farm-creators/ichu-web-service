namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Carries a ranch observation window and source completeness information for analytics.
/// </summary>
public sealed record AnalyticsSnapshot(
    Guid RanchId, DateRange Period, DateTimeOffset UpdatedAt, string Source, bool PopulationComplete,
    bool AlertsComplete, bool GeofencesComplete, bool CasesComplete,
    IReadOnlyDictionary<string, int> ConditionIncubationDays, int? ExpectedReadings,
    IReadOnlyList<AnimalSnapshot> Animals, IReadOnlyList<ReadingSnapshot> Readings, IReadOnlyList<AlertSnapshot> Alerts,
    IReadOnlyList<HealthCaseSnapshot> Cases)
{
    /// <summary>
    ///     Gets the recorded birth count, or null when observations are unavailable.
    /// </summary>
    public int? BirthCount { get; init; }

    /// <summary>
    ///     Gets the confirmed pregnancy count, or null when observations are unavailable.
    /// </summary>
    public int? ConfirmedPregnancies { get; init; }
}
