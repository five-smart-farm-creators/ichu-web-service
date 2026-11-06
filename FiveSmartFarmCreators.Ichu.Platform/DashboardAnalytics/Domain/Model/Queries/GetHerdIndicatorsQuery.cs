using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.Queries;

/// <summary>
///     Requests herd indicators for a ranch observation window.
/// </summary>
public sealed record GetHerdIndicatorsQuery(Guid RanchId, DateRange Period);
