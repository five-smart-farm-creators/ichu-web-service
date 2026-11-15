using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public sealed class WaterTroughReadingRepository(AppDbContext context) : IWaterTroughReadingRepository
{
    private IQueryable<WaterTroughReading> Readings => context.Set<WaterTroughReading>().AsNoTracking();

    public async Task AddAsync(WaterTroughReading reading, CancellationToken cancellationToken) =>
        await context.Set<WaterTroughReading>().AddAsync(reading, cancellationToken);

    public Task<WaterTroughReading?> FindByControllerAndReadingIdAsync(Guid controllerId, Guid readingId,
        CancellationToken cancellationToken) =>
        Readings.SingleOrDefaultAsync(reading => reading.ControllerId == controllerId && reading.ReadingId == readingId,
            cancellationToken);

    public async Task<IReadOnlyList<WaterTroughReading>> FindByControllerAsync(Guid ranchId, Guid controllerId,
        TimePeriod period, int skip, int take, CancellationToken cancellationToken) =>
        await InPeriod(ranchId, controllerId, period).OrderBy(reading => reading.CapturedAt)
            .ThenBy(reading => reading.ReadingId).Skip(skip).Take(take).ToListAsync(cancellationToken);

    public Task<int> CountByControllerAsync(Guid ranchId, Guid controllerId, TimePeriod period,
        CancellationToken cancellationToken) =>
        InPeriod(ranchId, controllerId, period).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<WaterTroughReading>> FindLatestByRanchAsync(Guid ranchId,
        CancellationToken cancellationToken) =>
        await Readings.Where(reading => reading.RanchId == ranchId)
            .GroupBy(reading => new { reading.ControllerId, reading.WaterTroughId })
            .Select(group => group.OrderByDescending(reading => reading.CapturedAt)
                .ThenByDescending(reading => reading.ReadingId).First())
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<WaterTroughReading>> FindLatestReceivedByRanchAsync(Guid ranchId,
        CancellationToken cancellationToken) =>
        await Readings.Where(reading => reading.RanchId == ranchId)
            .GroupBy(reading => new { reading.ControllerId, reading.WaterTroughId })
            .Select(group => group.OrderByDescending(reading => reading.ReceivedAt)
                .ThenByDescending(reading => reading.ReadingId).First())
            .ToListAsync(cancellationToken);

    private IQueryable<WaterTroughReading> InPeriod(Guid ranchId, Guid controllerId, TimePeriod period) =>
        Readings.Where(reading => reading.RanchId == ranchId && reading.ControllerId == controllerId
            && reading.CapturedAt >= period.From && reading.CapturedAt < period.To);
}
