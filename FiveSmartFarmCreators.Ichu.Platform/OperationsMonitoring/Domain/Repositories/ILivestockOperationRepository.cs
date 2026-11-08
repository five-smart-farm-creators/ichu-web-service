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
}
