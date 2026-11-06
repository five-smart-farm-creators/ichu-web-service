using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

public interface ICattleRepository
{
    Task<Cattle?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken = default);
    Task<Cattle?> FindByEarTagAsync(Guid ranchId, string earTag, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cattle>> SearchAsync(SearchCattleQuery query, CancellationToken cancellationToken = default);
    Task AddAsync(Cattle cattle, CancellationToken cancellationToken = default);
}
