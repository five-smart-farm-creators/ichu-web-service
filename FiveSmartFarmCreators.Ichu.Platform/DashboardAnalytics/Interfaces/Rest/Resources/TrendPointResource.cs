namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

/// <summary>
///     Represents the REST response data for trend point.
/// </summary>
public sealed record TrendPointResource(Guid AnimalId, DateTimeOffset RecordedAt, double Value);
