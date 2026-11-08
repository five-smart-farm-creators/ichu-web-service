namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

public sealed record ReadingSnapshot(Guid AnimalId, DateTimeOffset RecordedAt,
    double? BodyTemperature, double? ActivityIndex, double? RuminationMinutes);
