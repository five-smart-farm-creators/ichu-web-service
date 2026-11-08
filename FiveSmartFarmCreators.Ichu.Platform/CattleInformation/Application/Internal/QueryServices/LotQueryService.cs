using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;

/// <summary>
///     Checks ranch read access before retrieving lots and their stored memberships.
/// </summary>
public sealed class LotQueryService(ILotRepository lots, ICattleAccessService accessService) : ILotQueryService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Lot>>> HandleAsync(GetLotsByRanchQuery query, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<Lot>>.Failure(access.DomainError);
        }

        if (query.RanchId == Guid.Empty)
        {
            return Result<IReadOnlyList<Lot>>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        return Result<IReadOnlyList<Lot>>.Success(await lots.FindByRanchAsync(query.RanchId, cancellationToken));
    }
}
