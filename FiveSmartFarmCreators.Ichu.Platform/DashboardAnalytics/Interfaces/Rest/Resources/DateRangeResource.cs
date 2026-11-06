namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for date range.
/// </summary>
public sealed record DateRangeResource(DateTimeOffset From, DateTimeOffset To);
