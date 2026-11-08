using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.QueryServices;

/// <summary>
///     Composes authorized animal profiles from local identity and external device data without storing copies.
/// </summary>
public sealed class CattleQueryService(
    ICattleRepository cattles,
    ICattleAccessService accessService,
    ICattleProfileIntegrationService profileIntegrationService) : ICattleQueryService
{
    /// <inheritdoc />
    public async Task<Result<CattleProfile>> HandleAsync(GetCattleByIdQuery query, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<CattleProfile>.Failure(access.DomainError);
        }

        if (query.RanchId == Guid.Empty)
        {
            return Result<CattleProfile>.Failure(CattleInformationErrors.RanchIdentifierRequired);
        }

        if (query.CattleId.Value == Guid.Empty)
        {
            return Result<CattleProfile>.Failure(CattleInformationErrors.CattleIdentifierRequired);
        }

        var cattle = await cattles.FindByIdAsync(query.RanchId, query.CattleId, cancellationToken);
        if (cattle is null)
        {
            return Result<CattleProfile>.Failure(CattleInformationErrors.CattleNotFound);
        }

        var device = await profileIntegrationService.GetAsync(query.RanchId, query.CattleId.Value, cancellationToken);
        return Result<CattleProfile>.Success(new CattleProfile(cattle, device));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<Cattle>>> HandleAsync(SearchCattleQuery query, CancellationToken cancellationToken)
    {
        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<Cattle>>.Failure(access.DomainError);
        }

        if (query.RanchId == Guid.Empty || query.Page < 1 || query.PageSize is < 1 or > 100
            || (long)(query.Page - 1) * query.PageSize > int.MaxValue
            || query.LotId?.Value == Guid.Empty
            || (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
            || (!string.IsNullOrWhiteSpace(query.EarTag) && EarTag.Create(query.EarTag).IsFailure))
        {
            return Result<IReadOnlyList<Cattle>>.Failure(CattleInformationErrors.InvalidSearch);
        }

        return Result<IReadOnlyList<Cattle>>.Success(await cattles.SearchAsync(query, cancellationToken));
    }
}
