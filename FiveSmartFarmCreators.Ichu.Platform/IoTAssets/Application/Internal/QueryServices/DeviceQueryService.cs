using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.QueryServices;

public class DeviceQueryService(IIoTDeviceRepository deviceRepository, IIoTAssetsAccessService accessService)
    : IDeviceQueryService
{
    public async Task<Result<IoTDevice>> HandleAsync(GetDeviceByIdQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty || query.DeviceId == Guid.Empty)
        {
            return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IoTDevice>.Failure(access.DomainError);
        }

        var device = await deviceRepository.FindByIdAsNoTrackingAsync(query.RanchId, query.DeviceId, cancellationToken);
        return device is null
            ? Result<IoTDevice>.Failure(IoTAssetsErrors.DeviceNotFound)
            : Result<IoTDevice>.Success(device);
    }

    public async Task<Result<IReadOnlyList<IoTDevice>>> HandleAsync(GetDeviceInventoryQuery query, CancellationToken cancellationToken)
    {
        if (query.RanchId == Guid.Empty)
        {
            return Result<IReadOnlyList<IoTDevice>>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(query.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IReadOnlyList<IoTDevice>>.Failure(access.DomainError);
        }

        return Result<IReadOnlyList<IoTDevice>>.Success(
            await deviceRepository.FindByRanchAsNoTrackingAsync(query.RanchId, cancellationToken));
    }

    public async Task<Result<IReadOnlyList<DeviceAssignment>>> HandleAsync(GetDeviceAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var device = await HandleAsync(new GetDeviceByIdQuery(query.RanchId, query.DeviceId), cancellationToken);
        return device.IsFailure
            ? Result<IReadOnlyList<DeviceAssignment>>.Failure(device.DomainError)
            : Result<IReadOnlyList<DeviceAssignment>>.Success(device.Value!.Assignments.OrderBy(a => a.StartedAt).ThenBy(a => a.Id).ToArray());
    }
}
