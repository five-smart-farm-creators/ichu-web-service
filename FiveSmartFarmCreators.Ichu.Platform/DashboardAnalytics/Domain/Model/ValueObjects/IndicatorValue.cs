namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record IndicatorValue(double? Value, string Unit, string Formula, int? Denominator, string? UnavailableReason);
