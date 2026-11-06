namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for epidemiological curve.
/// </summary>
public sealed record EpidemiologicalCurveResource(Guid RanchId, DateRangeResource Period, DateTimeOffset UpdatedAt, string Source,
    string Condition, IReadOnlyDictionary<DateOnly, int> NewCasesByDay, int? DaysWithoutNewCases,
    bool? SupportsRestrictionLifting, string? UnavailableReason);
