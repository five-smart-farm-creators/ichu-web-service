using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Repository implementation for <see cref="TelemetryReading"/> aggregates using Entity Framework Core.
/// </summary>
/// <param name="context">The shared database context.</param>
public sealed class TelemetryReadingRepository(DbContext context) : ITelemetryReadingRepository
{
    private IQueryable<TelemetryReading> Readings => context.Set<TelemetryReading>().AsNoTracking();

    /// <inheritdoc />
    public async Task AddAsync(TelemetryReading reading, CancellationToken cancellationToken = default)
    {
        await context.Set<TelemetryReading>().AddAsync(reading, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TelemetryReading?> FindByDeviceAndReadingIdAsync(Guid deviceId, Guid readingId, CancellationToken cancellationToken = default) =>
        Readings.SingleOrDefaultAsync(r => r.DeviceId == deviceId && r.ReadingId == readingId, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TelemetryReading>> FindByDeviceAndReadingIdsAsync(Guid deviceId, IReadOnlyCollection<Guid> readingIds, CancellationToken cancellationToken = default) =>
        await Readings.Where(r => r.DeviceId == deviceId && readingIds.Contains(r.ReadingId)).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TelemetryReading>> FindByCattleAsync(Guid cattleId, TimePeriod period, int skip, int take, CancellationToken cancellationToken = default) =>
        await InPeriod(cattleId, period)
            .OrderBy(r => r.RecordedAt)
            .ThenBy(r => r.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<int> CountByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default) =>
        InPeriod(cattleId, period).CountAsync(cancellationToken);

    /// <inheritdoc />
    public Task<DateTimeOffset?> FindOldestRecordedAtByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default) =>
        Readings.Where(r => r.CattleId == cattleId).MinAsync(r => (DateTimeOffset?)r.RecordedAt, cancellationToken);

    /// <inheritdoc />
    public Task<TelemetryReading?> FindLatestByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default) =>
        Latest(Readings.Where(r => r.CattleId == cattleId), cancellationToken);

    /// <inheritdoc />
    public Task<TelemetryReading?> FindLatestValidByCattleAsync(Guid cattleId, TelemetryField field, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public Task<TelemetryReading?> FindLatestByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default) =>
        Latest(Readings.Where(r => r.DeviceId == deviceId), cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<TelemetryReading>> FindAttributedByRanchAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default) =>
        await Readings
            .Where(r => r.RanchId == ranchId && r.CattleId != null && r.RecordedAt >= period.From && r.RecordedAt < period.To)
            .OrderBy(r => r.RecordedAt)
            .ToListAsync(cancellationToken);

    private IQueryable<TelemetryReading> InPeriod(Guid cattleId, TimePeriod period) =>
        Readings.Where(r => r.CattleId == cattleId && r.RecordedAt >= period.From && r.RecordedAt < period.To);

    private static Task<TelemetryReading?> Latest(IQueryable<TelemetryReading> readings, CancellationToken cancellationToken) =>
        readings.OrderByDescending(r => r.RecordedAt).ThenByDescending(r => r.ReceivedAt).FirstOrDefaultAsync(cancellationToken);
}
