using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.QueryServices;

public class EdgeGatewayQueryService(IEdgeGatewayRepository gatewayRepository, IIoTAssetsAccessService accessService)
    : IEdgeGatewayQueryService
{
    public async Task<Result<EdgeGateway>> HandleAsync(GetEdgeGatewayByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty || query.GatewayId.Value == Guid.Empty)
        {
            return Result<EdgeGateway>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<EdgeGateway>.Failure(access.DomainError);
        }

        var gateway = await gatewayRepository.FindByIdAsNoTrackingAsync(query.RanchId, query.GatewayId, cancellationToken);
        return gateway is null
            ? Result<EdgeGateway>.Failure(IoTAssetsErrors.EdgeGatewayNotFound)
            : Result<EdgeGateway>.Success(gateway);
    }

    public async Task<Result<IReadOnlyList<EdgeGateway>>> HandleAsync(GetEdgeGatewaysByRanchQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty)
        {
            return Result<IReadOnlyList<EdgeGateway>>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<EdgeGateway>>.Failure(access.DomainError);
        }

        return Result<IReadOnlyList<EdgeGateway>>.Success(
            await gatewayRepository.FindByRanchAsNoTrackingAsync(query.RanchId, cancellationToken));
    }
}
