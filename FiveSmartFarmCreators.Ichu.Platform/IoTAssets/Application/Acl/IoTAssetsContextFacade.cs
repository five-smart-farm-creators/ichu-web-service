using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Acl;

/// <summary>
///     Serves device facts to other contexts behind the caller's ranch access, as snapshots that expose plain identifiers.
/// </summary>
public class IoTAssetsContextFacade(
    IDeviceCommandService deviceCommandService,
    IDeviceQueryService deviceQueryService,
    IWaterTroughQueryService waterTroughQueryService,
    IIoTDeviceRepository deviceRepository,
    IEdgeGatewayRepository edgeGatewayRepository,
    IIoTAssetsAccessService accessService,
    TimeProvider timeProvider) : IIoTAssetsContextFacade
{
    public async Task<Result<DeviceAssignmentSnapshot?>> GetCurrentCollarAsync(Guid ranchId, Guid cattleId,
        CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty || cattleId == Guid.Empty)
        {
            return Result<DeviceAssignmentSnapshot?>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureReadAccessAsync(ranchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<DeviceAssignmentSnapshot?>.Failure(access.DomainError);
        }

        var device = await deviceRepository.FindByAssignedCattleAsNoTrackingAsync(ranchId, cattleId, cancellationToken);
        var assignment = device?.Assignments.Single(a => a.IsOpen());
        return Result<DeviceAssignmentSnapshot?>.Success(assignment is null ? null : ToSnapshot(assignment));
    }

    public async Task<Result<DeviceAssignmentSnapshot?>> ResolveAssignmentAtAsync(Guid ranchId, Guid deviceId,
        DateTimeOffset instant, CancellationToken cancellationToken)
    {
        if (instant == default || instant > timeProvider.GetUtcNow())
        {
            return Result<DeviceAssignmentSnapshot?>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var result = await deviceQueryService.HandleAsync(new GetDeviceByIdQuery(ranchId, new DeviceId(deviceId)), cancellationToken);
        if (result.IsFailure)
        {
            return Result<DeviceAssignmentSnapshot?>.Failure(result.DomainError);
        }

        var assignment = result.Value!.Assignments.SingleOrDefault(a => a.CoversInstant(instant));
        return Result<DeviceAssignmentSnapshot?>.Success(assignment is null ? null : ToSnapshot(assignment));
    }

    public async Task<Result<IReadOnlyList<DeviceAssignmentSnapshot>>> GetActiveAssignmentsAsync(Guid ranchId,
        CancellationToken cancellationToken)
    {
        var result = await deviceQueryService.HandleAsync(new GetDeviceInventoryQuery(ranchId), cancellationToken);
        if (result.IsFailure)
        {
            return Result<IReadOnlyList<DeviceAssignmentSnapshot>>.Failure(result.DomainError);
        }

        var assignments = result.Value!.SelectMany(d => d.Assignments).Where(a => a.IsOpen())
            .OrderBy(a => a.StartedAt).ThenBy(a => a.Id.Value).Select(ToSnapshot).ToArray();
        return Result<IReadOnlyList<DeviceAssignmentSnapshot>>.Success(assignments);
    }

    public async Task<Result<WaterTroughConfiguration>> GetWaterTroughConfigurationAsync(Guid ranchId, Guid waterTroughId,
        CancellationToken cancellationToken)
    {
        var result = await waterTroughQueryService.HandleAsync(
            new GetWaterTroughByIdQuery(ranchId, new WaterTroughId(waterTroughId)), cancellationToken);
        if (result.IsFailure)
        {
            return Result<WaterTroughConfiguration>.Failure(result.DomainError);
        }

        var trough = result.Value!;
        return Result<WaterTroughConfiguration>.Success(new WaterTroughConfiguration(trough.Id.Value, trough.RanchId,
            trough.Name, trough.LotId, trough.TemperatureRange?.MinimumCelsius, trough.TemperatureRange?.MaximumCelsius,
            trough.Version));
    }

    public async Task<Result> RegisterCommunicationAsync(Guid ranchId, Guid deviceId, DateTimeOffset communicatedAt,
        DateTimeOffset receivedAt, int? batteryPercentage, CancellationToken cancellationToken)
    {
        var result = await deviceCommandService.HandleAsync(new RegisterDeviceCommunicationCommand(
            ranchId, new DeviceId(deviceId), communicatedAt, receivedAt, batteryPercentage), cancellationToken);
        return result.IsSuccess ? Result.Success() : Result.Failure(result.DomainError);
    }

    public Task<Result<int>> ReleaseCollarForInactiveCattleAsync(Guid ranchId, Guid cattleId,
        DateTimeOffset deactivatedAt, CancellationToken cancellationToken)
    {
        return deviceCommandService.HandleAsync(new ReleaseCollarForInactiveCattleCommand(ranchId, cattleId, deactivatedAt),
            cancellationToken);
    }

    public async Task<EdgeGatewaySnapshot?> AuthenticateEdgeGatewayAsync(string presentedKey, CancellationToken cancellationToken)
    {
        // Base64url secrets never contain a dot, so the first dot separates the gateway identifier from the secret.
        var separator = presentedKey.IndexOf('.');
        if (separator <= 0 || !Guid.TryParseExact(presentedKey[..separator], "D", out var gatewayId))
        {
            return null;
        }

        var gateway = await edgeGatewayRepository.FindByIdAsNoTrackingAsync(new EdgeGatewayId(gatewayId), cancellationToken);
        return gateway is not null && gateway.Authenticates(presentedKey[(separator + 1)..])
            ? new EdgeGatewaySnapshot(gateway.Id.Value, gateway.RanchId)
            : null;
    }

    public async Task<DeviceAttributionSnapshot?> FindDeviceAttributionAsync(Guid deviceId, DateTimeOffset instant,
        CancellationToken cancellationToken)
    {
        var device = await deviceRepository.FindByIdAsNoTrackingAsync(new DeviceId(deviceId), cancellationToken);
        if (device is null)
        {
            return null;
        }

        var assignment = device.Assignments.SingleOrDefault(a => a.CoversInstant(instant));
        var cattleId = assignment is { TargetType: AssignmentTargetType.Cattle } ? assignment.TargetId : (Guid?)null;
        return new DeviceAttributionSnapshot(device.Id.Value, device.RanchId, cattleId);
    }

    private static DeviceAssignmentSnapshot ToSnapshot(DeviceAssignment assignment) =>
        new(assignment.Id.Value, assignment.DeviceId.Value, assignment.RanchId, assignment.TargetId,
            assignment.TargetType.ToString(), assignment.StartedAt, assignment.EndedAt);
}
