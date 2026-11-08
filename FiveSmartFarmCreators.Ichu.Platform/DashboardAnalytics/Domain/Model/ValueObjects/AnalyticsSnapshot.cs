namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record AnalyticsSnapshot(
    Guid RanchId, DateRange Period, DateTimeOffset UpdatedAt, string Source, bool PopulationComplete,
    bool AlertsComplete, bool GeofencesComplete, bool CasesComplete,
    IReadOnlyDictionary<string, int> ConditionIncubationDays, int? ExpectedReadings,
    IReadOnlyList<AnimalSnapshot> Animals, IReadOnlyList<ReadingSnapshot> Readings, IReadOnlyList<AlertSnapshot> Alerts,
    IReadOnlyList<HealthCaseSnapshot> Cases)
{
    /// <summary>
    ///     Null marks births that were not observed, so an unavailable source never reads as zero.
    /// </summary>
    public int? BirthCount { get; init; }

    /// <summary>
    ///     Null marks confirmations that were not observed, because no context records them yet.
    /// </summary>
    public int? ConfirmedPregnancies { get; init; }
}
