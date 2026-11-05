using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence;

public sealed class TelemetryReadingRepository(DbContext context) : ITelemetryReadingRepository
{
    private IQueryable<TelemetryReading> Readings => context.Set<TelemetryReading>().AsNoTracking();

    public Task<TelemetryReading?> FindAsync(Guid deviceId, Guid readingId, CancellationToken cancellationToken) =>
        Readings.SingleOrDefaultAsync(r => r.DeviceId == deviceId && r.ReadingId == readingId, cancellationToken);

    public async Task<IReadOnlyList<TelemetryReading>> FindManyAsync(Guid deviceId, IReadOnlyCollection<Guid> readingIds, CancellationToken cancellationToken) =>
        await Readings.Where(r => r.DeviceId == deviceId && readingIds.Contains(r.ReadingId)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TelemetryReading>> FindByCattleAsync(Guid cattleId, TimePeriod period, int skip, int take, CancellationToken cancellationToken) =>
        await InPeriod(cattleId, period)
            .OrderBy(r => r.RecordedAt)
            .ThenBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken) =>
        InPeriod(cattleId, period).CountAsync(cancellationToken);

    public Task<DateTimeOffset?> FindOldestRecordedAtAsync(Guid cattleId, CancellationToken cancellationToken) =>
        Readings.Where(r => r.CattleId == cattleId).MinAsync(r => (DateTimeOffset?)r.RecordedAt, cancellationToken);

    public Task<TelemetryReading?> FindLatestAsync(Guid cattleId, CancellationToken cancellationToken) =>
        Latest(Readings.Where(r => r.CattleId == cattleId), cancellationToken);

    public Task<TelemetryReading?> FindLatestValidAsync(Guid cattleId, TelemetryField field, CancellationToken cancellationToken)
    {
        var readings = Readings.Where(r => r.CattleId == cattleId);
        readings = field switch
        {
            TelemetryField.BodyTemperature => readings.Where(r => r.BodyTemperature.Value != null),
            TelemetryField.ActivityIndex => readings.Where(r => r.ActivityIndex.Value != null),
            TelemetryField.RuminationMinutes => readings.Where(r => r.RuminationMinutes.Value != null),
            TelemetryField.BatteryPercent => readings.Where(r => r.BatteryPercent.Value != null),
            TelemetryField.Position => readings.Where(r => r.Position.Latitude != null),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };
        return Latest(readings, cancellationToken);
    }

    public Task<TelemetryReading?> FindLatestForDeviceAsync(Guid deviceId, CancellationToken cancellationToken) =>
        Latest(Readings.Where(r => r.DeviceId == deviceId), cancellationToken);

    public async Task<IReadOnlyList<TelemetryReading>> FindAttributedByRanchAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken) =>
        await Readings
            .Where(r => r.RanchId == ranchId && r.CattleId != null && r.RecordedAt >= period.From && r.RecordedAt < period.To)
            .OrderBy(r => r.RecordedAt)
            .ToListAsync(cancellationToken);

    public void Add(TelemetryReading reading) => context.Set<TelemetryReading>().Add(reading);

    private IQueryable<TelemetryReading> InPeriod(Guid cattleId, TimePeriod period) =>
        Readings.Where(r => r.CattleId == cattleId && r.RecordedAt >= period.From && r.RecordedAt < period.To);

    private static Task<TelemetryReading?> Latest(IQueryable<TelemetryReading> readings, CancellationToken cancellationToken) =>
        readings.OrderByDescending(r => r.RecordedAt).ThenByDescending(r => r.ReceivedAt).FirstOrDefaultAsync(cancellationToken);
}
