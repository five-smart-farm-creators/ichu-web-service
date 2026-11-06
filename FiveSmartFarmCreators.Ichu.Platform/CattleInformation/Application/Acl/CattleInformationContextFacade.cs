using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Acl;

public sealed class CattleInformationContextFacade(ICattleRepository cattleRepository, ICattleAccessService accessService)
    : ICattleInformationContextFacade
{
    public async Task<CattleSnapshot?> FindByIdAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        await accessService.EnsureReadAccessAsync(ranchId, cancellationToken);
        ValidateIdentifiers(ranchId, cattleId);
        var cattle = await cattleRepository.FindByIdAsync(ranchId, cattleId, cancellationToken);
        return cattle is null ? null : ToSnapshot(cattle);
    }

    public async Task<IReadOnlyList<CattleSnapshot>> FindActiveByLotAsync(Guid ranchId, Guid lotId,
        CancellationToken cancellationToken)
    {
        await accessService.EnsureReadAccessAsync(ranchId, cancellationToken);
        ValidateIdentifiers(ranchId, lotId);
        var snapshots = new List<CattleSnapshot>();
        var page = 1;
        while (true)
        {
            var query = new SearchCattleQuery(ranchId, Status: CattleStatus.Active, LotId: lotId, Page: page, PageSize: 100);
            var cattle = await cattleRepository.SearchAsync(query, cancellationToken);
            snapshots.AddRange(cattle.Select(ToSnapshot));
            if (cattle.Count < query.PageSize)
            {
                return snapshots;
            }

            page++;
        }
    }

    private static CattleSnapshot ToSnapshot(Cattle cattle) => new(
        cattle.Id, cattle.RanchId, cattle.EarTag.Value, cattle.Status.ToString(), cattle.CurrentStage.ToString(),
        cattle.BirthDate, cattle.ExitReason?.ToString(), cattle.ExitDate, cattle.Version);

    private static void ValidateIdentifiers(Guid ranchId, Guid resourceId)
    {
        if (ranchId == Guid.Empty || resourceId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "Ranch and resource identifiers are required.");
        }
    }
}
