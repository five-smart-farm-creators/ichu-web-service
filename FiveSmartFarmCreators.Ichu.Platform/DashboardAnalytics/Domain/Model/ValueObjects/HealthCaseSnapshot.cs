namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes the onset of a recorded animal health case.
/// </summary>
public sealed record HealthCaseSnapshot(Guid CaseId, Guid AnimalId, string Condition, DateTimeOffset Onset);
