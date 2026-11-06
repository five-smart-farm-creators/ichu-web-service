using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

public interface ILotRepository
{
    Task<Lot?> FindByIdAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Lot>> FindByRanchAsync(Guid ranchId, CancellationToken cancellationToken = default);
    Task AddAsync(Lot lot, CancellationToken cancellationToken = default);
}
