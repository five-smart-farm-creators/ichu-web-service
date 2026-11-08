namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record EpidemiologicalCurve(Guid RanchId, DateRange Period, DateTimeOffset UpdatedAt, string Source,
    string Condition, IReadOnlyDictionary<DateOnly, int> NewCasesByDay, int? DaysWithoutNewCases,
    bool? SupportsRestrictionLifting, string? UnavailableReason);
