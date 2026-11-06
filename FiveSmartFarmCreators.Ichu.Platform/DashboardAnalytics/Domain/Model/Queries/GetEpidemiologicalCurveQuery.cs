using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

/// <summary>
///     Requests daily case onset counts for a ranch and health condition.
/// </summary>
public sealed record GetEpidemiologicalCurveQuery(Guid RanchId, DateRange Period, string Condition);
