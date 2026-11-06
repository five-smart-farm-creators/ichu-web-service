namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for indicator value.
/// </summary>
public sealed record IndicatorValueResource(double? Value, string Unit, string Formula, int? Denominator, string? UnavailableReason);
