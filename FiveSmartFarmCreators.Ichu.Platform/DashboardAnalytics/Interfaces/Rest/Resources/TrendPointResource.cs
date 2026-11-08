namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record TrendPointResource(Guid AnimalId, DateTimeOffset RecordedAt, double Value);
