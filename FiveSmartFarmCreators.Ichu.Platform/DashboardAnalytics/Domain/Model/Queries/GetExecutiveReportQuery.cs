using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

/// <summary>
///     Requests a subscription-authorized executive report in JSON or CSV format.
/// </summary>
public sealed record GetExecutiveReportQuery(Guid RanchId, DateRange Period, string Format);
