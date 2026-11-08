using FiveSmartFarmCreators.Ichu.Platform.DashboardAnalytics.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.Tests.DashboardAnalytics;

internal static class AnalyticsFixtures
{
    public static readonly Guid RanchId = Guid.Parse("11111111-1111-4111-8111-111111111111");
    public static readonly Guid LotId = Guid.Parse("22222222-2222-4222-8222-222222222222");
    public static readonly DateTimeOffset PeriodEnd = new(2026, 12, 1, 0, 0, 0, TimeSpan.Zero);
    public static readonly DateRange Period = DateRange.Create(Day(1), PeriodEnd).Value!;

    public static DateTimeOffset Day(int day) => new(2026, 11, day, 0, 0, 0, TimeSpan.Zero);

    public static AnalyticsSnapshot Snapshot(
        IReadOnlyList<AnimalSnapshot>? animals = null,
        IReadOnlyList<ReadingSnapshot>? readings = null,
        IReadOnlyList<AlertSnapshot>? alerts = null,
        IReadOnlyList<HealthCaseSnapshot>? cases = null,
        DateRange? period = null,
        DateTimeOffset? updatedAt = null,
        bool populationComplete = true) =>
        new(RanchId, period ?? Period, updatedAt ?? PeriodEnd, "ichu-platform", populationComplete, true, true, true,
            new Dictionary<string, int> { ["temperature"] = 7, ["activity"] = 7 }, null,
            animals ?? Array.Empty<AnimalSnapshot>(), readings ?? Array.Empty<ReadingSnapshot>(),
            alerts ?? Array.Empty<AlertSnapshot>(), cases ?? Array.Empty<HealthCaseSnapshot>());

    public static AnimalSnapshot Animal(Guid animalId, bool died = false, bool outsideGeofence = false) =>
        new(animalId, LotId, died, outsideGeofence);

    public static ReadingSnapshot Temperature(Guid animalId, DateTimeOffset recordedAt, double temperature) =>
        new(animalId, recordedAt, temperature, null, null);
}
