namespace FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

/// <summary>
///     Describes telemetry observations used to calculate animal and lot trends.
/// </summary>
public sealed record ReadingSnapshot(Guid AnimalId, DateTimeOffset RecordedAt,
    double? BodyTemperature, double? ActivityIndex, double? RuminationMinutes);
