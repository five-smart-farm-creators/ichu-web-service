namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record HealthCaseSnapshot(Guid CaseId, Guid AnimalId, string Condition, DateTimeOffset Onset);
