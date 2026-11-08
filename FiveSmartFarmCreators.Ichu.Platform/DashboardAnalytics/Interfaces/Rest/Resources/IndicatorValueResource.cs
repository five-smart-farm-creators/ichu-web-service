namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record IndicatorValueResource(double? Value, string Unit, string Formula, int? Denominator, string? UnavailableReason);
