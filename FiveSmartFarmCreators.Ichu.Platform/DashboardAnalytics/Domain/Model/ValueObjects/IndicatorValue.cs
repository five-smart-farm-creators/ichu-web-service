namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes a measured indicator with its formula, denominator, and availability.
/// </summary>
public sealed record IndicatorValue(double? Value, string Unit, string Formula, int? Denominator, string? UnavailableReason);
