using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

public sealed record GetEpidemiologicalCurveQuery(Guid RanchId, DateRange Period, string Condition);
