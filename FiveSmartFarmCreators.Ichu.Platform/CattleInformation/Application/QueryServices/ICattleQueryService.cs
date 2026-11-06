using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;

public interface ICattleQueryService
{
    Task<CattleProfile> HandleAsync(GetCattleByIdQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<Cattle>> HandleAsync(SearchCattleQuery query, CancellationToken cancellationToken);
}
