namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Associates a valid metric value with an animal and observation timestamp.
/// </summary>
public sealed record TrendPoint(Guid AnimalId, DateTimeOffset RecordedAt, double Value);
