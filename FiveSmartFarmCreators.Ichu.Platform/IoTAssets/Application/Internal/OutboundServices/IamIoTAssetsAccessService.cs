using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>
///     Implements <see cref="IIoTAssetsAccessService"/> with the IAM context facade for user requests and with the device
///     repository for edge gateway reports.
/// </summary>
public class IamIoTAssetsAccessService(
    IIamContextFacade iamContextFacade,
    IIoTDeviceRepository deviceRepository) : IIoTAssetsAccessService
{
    /// <inheritdoc />
    public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Read, cancellationToken);

    /// <inheritdoc />
    public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Manage, cancellationToken);

    /// <inheritdoc />
    public async Task<Result> EnsureCommunicationAccessAsync(Guid ranchId, Guid deviceId, CancellationToken cancellationToken)
    {
        // Edge gateways authenticate at the Operations and Monitoring boundary, so no user account exists in this path.
        var device = await deviceRepository.FindByIdAsNoTrackingAsync(ranchId, new DeviceId(deviceId), cancellationToken);
        return device is null ? Result.Failure(IoTAssetsErrors.DeviceNotFound) : Result.Success();
    }

    private async Task<Result> EnsureAccessAsync(Guid ranchId, AccessScope scope, CancellationToken cancellationToken)
    {
        var accountId = await iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId)
        {
            return Result.Failure(IoTAssetsErrors.Unauthenticated);
        }

        return await iamContextFacade.CanAccessRanchAsync(verifiedAccountId, ranchId, scope, cancellationToken)
            ? Result.Success()
            : Result.Failure(IoTAssetsErrors.Forbidden);
    }
}
