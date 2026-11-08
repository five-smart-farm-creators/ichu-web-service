namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record AlertSnapshot(Guid AlertId, Guid AnimalId, string Type, DateTimeOffset RaisedAt, DateTimeOffset? ClosedAt);
