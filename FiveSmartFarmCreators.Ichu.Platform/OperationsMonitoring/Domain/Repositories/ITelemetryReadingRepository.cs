using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores telemetry readings and answers the queries that the read services need.
/// </summary>
public interface ITelemetryReadingRepository
{
    Task AddAsync(TelemetryReading reading, CancellationToken cancellationToken = default);

    Task<TelemetryReading?> FindByDeviceAndReadingIdAsync(Guid deviceId, Guid readingId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TelemetryReading>> FindByDeviceAndReadingIdsAsync(Guid deviceId, IReadOnlyCollection<Guid> readingIds, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds a page of the readings of an animal captured within a period, ordered by capture time.
    /// </summary>
    Task<IReadOnlyList<TelemetryReading>> FindByCattleAsync(Guid cattleId, TimePeriod period, int skip, int take, CancellationToken cancellationToken = default);

    Task<int> CountByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> FindOldestRecordedAtByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default);

    Task<TelemetryReading?> FindLatestByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the most recent reading of an animal that has a valid value for a field.
    /// </summary>
    Task<TelemetryReading?> FindLatestValidByCattleAsync(Guid cattleId, TelemetryField field, CancellationToken cancellationToken = default);

    Task<TelemetryReading?> FindLatestByDeviceAsync(Guid deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the readings of a ranch that are attributed to an animal and were captured within a period, ordered by capture time.
    /// </summary>
    Task<IReadOnlyList<TelemetryReading>> FindAttributedByRanchAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default);
}
