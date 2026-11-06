using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;

/// <summary>
/// Checks ranch read access before retrieving lots and their stored memberships.
/// </summary>
public sealed class LotQueryService(ILotRepository lotRepository, ICattleAccessService accessService) : ILotQueryService
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Lot>> HandleAsync(GetLotsByRanchQuery query, CancellationToken cancellationToken)
    {
        await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (query.RanchId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "A ranch identifier is required.");
        }

        return await lotRepository.FindByRanchAsync(query.RanchId, cancellationToken);
    }
}
