using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.CommandServices;

/// <summary>
///     Coordinates device changes. Each state change runs in one transaction with its version check, and the domain
///     aggregates decide whether the change is allowed.
/// </summary>
public class DeviceCommandService(
    IIoTDeviceRepository deviceRepository,
    IWaterTroughRepository waterTroughRepository,
    IIoTAssetsAccessService accessService,
    ICattleLookupPort cattleLookup,
    ISubscriptionEntitlementPort subscriptionEntitlement,
    IIoTAssetsTransaction transaction,
    DeviceAssignmentDomainService assignmentService,
    TimeProvider timeProvider) : IDeviceCommandService
{
    public async Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty)
        {
            return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(command.RanchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IoTDevice>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var registration = IoTDevice.Register(command, timeProvider.GetUtcNow());
            if (registration.IsFailure)
            {
                return registration;
            }

            var device = registration.Value!;
            if (await deviceRepository.ExistsBySerialAsync(device.Serial, token))
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.DuplicateSerial);
            }

            await deviceRepository.AddAsync(device, token);
            return Result<IoTDevice>.Success(device);
        }, cancellationToken);
    }

    public Task<Result<IoTDevice>> HandleAsync(AssignDeviceCommand command, CancellationToken cancellationToken)
    {
        return ExecuteForManagedDeviceAsync(command.RanchId, command.DeviceId, command.ExpectedVersion, async (device, token) =>
        {
            if (command.TargetId == Guid.Empty)
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
            }

            if (!device.IsAvailable())
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidState);
            }

            var targetType = device.Type == DeviceType.SmartCollar
                ? AssignmentTargetType.Cattle : AssignmentTargetType.WaterTrough;
            if (await deviceRepository.HasOpenAssignmentAsync(command.RanchId, command.TargetId, targetType, token))
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.TargetAlreadyAssigned);
            }

            var assignedCollars = 0;
            var entitlement = new CollarEntitlement(false, 0);
            if (device.Type == DeviceType.SmartCollar)
            {
                var cattle = await cattleLookup.GetCattleAsync(command.RanchId, command.TargetId, token);
                if (cattle.IsFailure)
                {
                    return Result<IoTDevice>.Failure(cattle.DomainError);
                }

                if (cattle.Value!.Id != command.TargetId || cattle.Value.RanchId != command.RanchId)
                {
                    return Result<IoTDevice>.Failure(IoTAssetsErrors.IntegrationUnavailable);
                }

                if (!cattle.Value.IsActive)
                {
                    return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidState);
                }

                var subscription = await subscriptionEntitlement.GetAsync(command.RanchId, token);
                if (subscription.IsFailure)
                {
                    return Result<IoTDevice>.Failure(subscription.DomainError);
                }

                entitlement = subscription.Value!;
                assignedCollars = await deviceRepository.CountAssignedCollarsAsync(command.RanchId, token);
            }
            else if (await waterTroughRepository.FindByIdAsync(command.RanchId, new WaterTroughId(command.TargetId), token) is null)
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.TargetNotFound);
            }

            var assignment = assignmentService.Assign(device, command.TargetId, assignedCollars,
                entitlement.MaximumCollars, entitlement.IsActive, timeProvider.GetUtcNow());
            return assignment.IsFailure
                ? Result<IoTDevice>.Failure(assignment.DomainError)
                : Result<IoTDevice>.Success(device);
        }, cancellationToken);
    }

    public Task<Result<IoTDevice>> HandleAsync(ReleaseDeviceCommand command, CancellationToken cancellationToken)
    {
        return ExecuteForManagedDeviceAsync(command.RanchId, command.DeviceId, command.ExpectedVersion, (device, _) =>
        {
            var release = device.Release(command.Reason, timeProvider.GetUtcNow());
            return Task.FromResult(release.IsFailure
                ? Result<IoTDevice>.Failure(release.DomainError)
                : Result<IoTDevice>.Success(device));
        }, cancellationToken);
    }

    public Task<Result<IoTDevice>> HandleAsync(SetDeviceMaintenanceCommand command, CancellationToken cancellationToken)
    {
        return ExecuteForManagedDeviceAsync(command.RanchId, command.DeviceId, command.ExpectedVersion, (device, _) =>
        {
            var change = device.SetMaintenance(command.InMaintenance, timeProvider.GetUtcNow());
            return Task.FromResult(change.IsFailure
                ? Result<IoTDevice>.Failure(change.DomainError)
                : Result<IoTDevice>.Success(device));
        }, cancellationToken);
    }

    public async Task<Result<IoTDevice>> HandleAsync(RegisterDeviceCommunicationCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty || command.DeviceId.Value == Guid.Empty)
        {
            return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureCommunicationAccessAsync(command.RanchId, command.DeviceId.Value, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IoTDevice>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var device = await deviceRepository.FindByIdAsync(command.RanchId, command.DeviceId, token);
            if (device is null)
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.DeviceNotFound);
            }

            BatteryLevel? battery = null;
            if (command.BatteryPercentage is { } percentage)
            {
                var level = BatteryLevel.Create(percentage);
                if (level.IsFailure)
                {
                    return Result<IoTDevice>.Failure(level.DomainError);
                }

                battery = level.Value;
            }

            var registration = device.RegisterCommunication(command.CommunicatedAt, command.ReceivedAt, battery, timeProvider.GetUtcNow());
            return registration.IsFailure
                ? Result<IoTDevice>.Failure(registration.DomainError)
                : Result<IoTDevice>.Success(device);
        }, cancellationToken);
    }

    // The cattle lookup confirms the animal is inactive in this ranch, which is the authorization for releasing its collar.
    public async Task<Result<int>> HandleAsync(ReleaseCollarForInactiveCattleCommand command, CancellationToken cancellationToken)
    {
        if (command.RanchId == Guid.Empty || command.CattleId == Guid.Empty
            || command.DeactivatedAt == default || command.DeactivatedAt > timeProvider.GetUtcNow())
        {
            return Result<int>.Failure(IoTAssetsErrors.InvalidInput);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var cattle = await cattleLookup.GetCattleAsync(command.RanchId, command.CattleId, token);
            if (cattle.IsFailure)
            {
                return Result<int>.Failure(cattle.DomainError);
            }

            if (cattle.Value!.Id != command.CattleId || cattle.Value.RanchId != command.RanchId)
            {
                return Result<int>.Failure(IoTAssetsErrors.IntegrationUnavailable);
            }

            if (cattle.Value.IsActive)
            {
                return Result<int>.Failure(IoTAssetsErrors.InvalidState);
            }

            var device = await deviceRepository.FindByAssignedCattleAsync(command.RanchId, command.CattleId, token);
            if (device is null)
            {
                return Result<int>.Success(0);
            }

            var assignment = device.Assignments.Single(item => item.IsOpen());
            var releasedAt = command.DeactivatedAt < assignment.StartedAt ? assignment.StartedAt : command.DeactivatedAt;
            var release = device.Release("Cattle deactivated", releasedAt);
            return release.IsFailure
                ? Result<int>.Failure(release.DomainError)
                : Result<int>.Success(1);
        }, cancellationToken);
    }

    private async Task<Result<IoTDevice>> ExecuteForManagedDeviceAsync(Guid ranchId, DeviceId deviceId, Guid expectedVersion,
        Func<IoTDevice, CancellationToken, Task<Result<IoTDevice>>> operation, CancellationToken cancellationToken)
    {
        if (ranchId == Guid.Empty || deviceId.Value == Guid.Empty || expectedVersion == Guid.Empty)
        {
            return Result<IoTDevice>.Failure(IoTAssetsErrors.InvalidInput);
        }

        var access = await accessService.EnsureManageAccessAsync(ranchId, cancellationToken);
        if (access.IsFailure)
        {
            return Result<IoTDevice>.Failure(access.DomainError);
        }

        return await transaction.ExecuteAsync(async token =>
        {
            var device = await deviceRepository.FindByIdAsync(ranchId, deviceId, token);
            if (device is null)
            {
                return Result<IoTDevice>.Failure(IoTAssetsErrors.DeviceNotFound);
            }

            var version = device.EnsureVersion(expectedVersion);
            return version.IsFailure
                ? Result<IoTDevice>.Failure(version.DomainError)
                : await operation(device, token);
        }, cancellationToken);
    }
}
