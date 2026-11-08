namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record TrendPoint(Guid AnimalId, DateTimeOffset RecordedAt, double Value);
