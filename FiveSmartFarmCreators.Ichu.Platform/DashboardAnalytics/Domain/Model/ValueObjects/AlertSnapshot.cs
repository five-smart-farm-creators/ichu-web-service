namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes an observed alert and its lifecycle for period-end analysis.
/// </summary>
public sealed record AlertSnapshot(Guid AlertId, Guid AnimalId, string Type, DateTimeOffset RaisedAt, DateTimeOffset? ClosedAt);
