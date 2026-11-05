using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Repositories;

public interface ILivestockOperationRepository
{
    Task<LivestockOperation?> FindAsync(Guid operationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<LivestockOperation>> FindByCattleAsync(Guid cattleId, TimePeriod period, CancellationToken cancellationToken);

    void Add(LivestockOperation operation);
}
