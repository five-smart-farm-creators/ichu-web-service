namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Interfaces.Rest.Resources;

public sealed record TrendPointResource(Guid CattleId, DateTimeOffset RecordedAt, double Value);
