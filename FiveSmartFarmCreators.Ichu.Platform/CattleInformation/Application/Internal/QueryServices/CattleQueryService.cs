using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;

/// <summary>
/// Composes authorized animal profiles using external device data without storing copies.
/// </summary>
public sealed class CattleQueryService(
    ICattleRepository cattleRepository,
    ICattleAccessService accessService,
    ICattleProfileIntegrationService profileIntegrationService) : ICattleQueryService
{
    /// <inheritdoc />
    public async Task<CattleProfile> HandleAsync(GetCattleByIdQuery query, CancellationToken cancellationToken)
    {
        await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (query.RanchId == Guid.Empty || query.CattleId == Guid.Empty)
        {
            throw new CattleInformationException("invalid_input", "Ranch and cattle identifiers are required.");
        }

        var cattle = await cattleRepository.FindByIdAsync(query.RanchId, query.CattleId, cancellationToken)
            ?? throw new CattleInformationException("not_found", "The animal was not found in this ranch.");
        var device = await profileIntegrationService.GetAsync(query.RanchId, query.CattleId, cancellationToken);
        return new CattleProfile(cattle, device);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Cattle>> HandleAsync(SearchCattleQuery query, CancellationToken cancellationToken)
    {
        await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (query.RanchId == Guid.Empty || query.Page < 1 || query.PageSize is < 1 or > 100
            || (long)(query.Page - 1) * query.PageSize > int.MaxValue
            || query.LotId == Guid.Empty || (query.Status.HasValue && !Enum.IsDefined(query.Status.Value)))
        {
            throw new CattleInformationException("invalid_input", "The search filters or pagination are invalid.");
        }

        return await cattleRepository.SearchAsync(query, cancellationToken);
    }
}
