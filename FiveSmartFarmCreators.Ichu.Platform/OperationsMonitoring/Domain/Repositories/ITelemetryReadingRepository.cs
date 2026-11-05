using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface ITelemetryReadingRepository
{
    Task<TelemetryReading?> FindAsync(Guid deviceId, Guid readingId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TelemetryReading>> FindManyAsync(Guid deviceId, IReadOnlyCollection<Guid> readingIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<TelemetryReading>> FindByCattleAsync(Guid cattleId, TimePeriod period, int skip, int take, CancellationToken cancellationToken);

    Task<int> CountByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken);

    Task<DateTimeOffset?> FindOldestRecordedAtAsync(Guid cattleId, CancellationToken cancellationToken);

    Task<TelemetryReading?> FindLatestAsync(Guid cattleId, CancellationToken cancellationToken);

    Task<TelemetryReading?> FindLatestValidAsync(Guid cattleId, TelemetryField field, CancellationToken cancellationToken);

    Task<TelemetryReading?> FindLatestForDeviceAsync(Guid deviceId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TelemetryReading>> FindAttributedByRanchAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken);

    void Add(TelemetryReading reading);
}
