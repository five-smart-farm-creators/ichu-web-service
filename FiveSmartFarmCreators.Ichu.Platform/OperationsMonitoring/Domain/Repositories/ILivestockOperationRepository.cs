using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

/// <summary>
///     Stores livestock operations.
/// </summary>
public interface ILivestockOperationRepository
{
    Task AddAsync(LivestockOperation operation, CancellationToken cancellationToken = default);

    Task<LivestockOperation?> FindByIdAsync(LivestockOperationId operationId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the operations applied to an animal within a period, ordered by occurrence time.
    /// </summary>
    Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds every operation applied to an animal, ordered by occurrence time, for its complete reproductive history.
    /// </summary>
    Task<IReadOnlyList<LivestockOperation>> FindAllByCattleAsync(Guid cattleId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Finds the operations of every animal of a ranch within a period, ordered by occurrence time, without tracking them.
    /// </summary>
    Task<IReadOnlyList<LivestockOperation>> FindByRanchAndPeriodAsNoTrackingAsync(Guid ranchId, TimePeriod period, CancellationToken cancellationToken = default);
}
