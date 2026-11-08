using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.QueryServices;

public class WaterTroughQueryService(IWaterTroughRepository waterTroughRepository, IIoTAssetsAccessService accessService)
    : IWaterTroughQueryService
{
    public async Task<Result<WaterTrough>> HandleAsync(GetWaterTroughByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty || query.WaterTroughId.Value == Guid.Empty)
        {
            return Result<WaterTrough>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<WaterTrough>.Failure(access.DomainError);
        }

        var waterTrough = await waterTroughRepository.FindByIdAsNoTrackingAsync(query.RanchId, query.WaterTroughId, cancellationToken);
        return waterTrough is null
            ? Result<WaterTrough>.Failure(IoTAssetsErrors.WaterTroughNotFound)
            : Result<WaterTrough>.Success(waterTrough);
    }

    public async Task<Result<IReadOnlyList<WaterTrough>>> HandleAsync(GetWaterTroughsQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty)
        {
            return Result<IReadOnlyList<WaterTrough>>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<WaterTrough>>.Failure(access.DomainError);
        }

        return Result<IReadOnlyList<WaterTrough>>.Success(
            await waterTroughRepository.FindByRanchAsNoTrackingAsync(query.RanchId, cancellationToken));
    }
}
