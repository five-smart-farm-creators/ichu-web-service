using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

public interface ILotQueryService
{
    Task<IReadOnlyList<Lot>> HandleAsync(GetLotsByRanchQuery query, CancellationToken cancellationToken);
}
