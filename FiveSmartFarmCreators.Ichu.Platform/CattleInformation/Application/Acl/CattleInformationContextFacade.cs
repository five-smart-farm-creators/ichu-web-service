using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Acl;

/// <summary>
///     Serves cross-context animal and lot queries behind the caller's ranch read access and returns immutable snapshots.
/// </summary>
public sealed class CattleInformationContextFacade(
    ICattleRepository cattles,
    ILotRepository lots,
    ICattleAccessService accessService) : ICattleInformationContextFacade
{
    /// <inheritdoc />
    public async Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        if (!await CanReadAsync(ranchId, cattleId, cancellationToken))
        {
            return null;
        }

        var cattle = await cattles.FindByIdAsync(ranchId, new CattleId(cattleId), cancellationToken);
        return cattle is null ? null : ToSnapshot(cattle);
    }

    /// <inheritdoc />
    public async Task<bool> LotExistsAsync(Guid ranchId, Guid lotId, CancellationToken cancellationToken)
    {
        if (!await CanReadAsync(ranchId, lotId, cancellationToken))
        {
            return false;
        }

        return await lots.FindByIdAsync(ranchId, new LotId(lotId), cancellationToken) is not null;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId,
        CancellationToken cancellationToken)
    {
        if (!await CanReadAsync(ranchId, lotId, cancellationToken))
        {
            return [];
        }

        var snapshots = new List<CattleSnapshot>();
        var page = 1;
        while (true)
        {
            var query = new SearchCattleQuery(ranchId, Status: CattleStatus.Active, LotId: new LotId(lotId), Page: page,
                PageSize: 100);
            var cattle = await cattles.SearchAsync(query, cancellationToken);
            snapshots.AddRange(cattle.Select(ToSnapshot));
            if (cattle.Count < query.PageSize)
            {
                return snapshots;
            }

            page++;
        }
    }

    // Unknown and unauthorized targets look the same to other contexts, so ids of other ranches cannot be probed.
    private async Task<bool> CanReadAsync(Guid ranchId, Guid resourceId, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty || resourceId == Guid.Empty)
        {
            return false;
        }

        var access = await accessService.EnsureReadAccessAsync(ranchId, cancellationToken);
        return access.IsSuccess;
    }

    private static CattleSnapshot ToSnapshot(Cattle cattle) => new(
        cattle.Id.Value, cattle.RanchId, cattle.EarTag.Value, cattle.Status.ToString(), cattle.CurrentStage.ToString(),
        cattle.BirthDate, cattle.ExitReason?.ToString(), cattle.ExitDate, cattle.Version);
}
