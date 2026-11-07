namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes daily case onset counts and the available incubation-period assessment.
/// </summary>
public sealed record EpidemiologicalCurve(Guid RanchId, DateRange Period, DateTimeOffset UpdatedAt, string Source,
    string Condition, IReadOnlyDictionary<DateOnly, int> NewCasesByDay, int? DaysWithoutNewCases,
    bool? SupportsRestrictionLifting, string? UnavailableReason);
