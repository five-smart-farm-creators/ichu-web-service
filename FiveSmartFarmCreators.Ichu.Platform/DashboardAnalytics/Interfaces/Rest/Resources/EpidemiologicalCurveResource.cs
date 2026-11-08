namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record EpidemiologicalCurveResource(Guid RanchId, DateRangeResource Period, DateTimeOffset UpdatedAt, string Source,
    string Condition, IReadOnlyDictionary<DateOnly, int> NewCasesByDay, int? DaysWithoutNewCases,
    bool? SupportsRestrictionLifting, string? UnavailableReason);
