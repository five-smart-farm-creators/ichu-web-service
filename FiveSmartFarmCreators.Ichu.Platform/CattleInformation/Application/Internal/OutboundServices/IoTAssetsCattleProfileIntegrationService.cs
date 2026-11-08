using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
///     Combines the current collar of an animal from IoT Assets with its last received reading from Operations and
///     Monitoring. An unavailable IoT answer is reported as such, never as a missing collar.
/// </summary>
public class IoTAssetsCattleProfileIntegrationService(
    IIoTAssetsContextFacade ioTAssets,
    IOperationsMonitoringContextFacade operationsMonitoring) : ICattleProfileIntegrationService
{
    /// <inheritdoc />
    public async Task<CattleDeviceDetails> GetAsync(Guid ranchId, Guid cattleId, CancellationToken cancellationToken)
    {
        var collar = await ioTAssets.GetCurrentCollarAsync(ranchId, cattleId, cancellationToken);
        if (collar.IsFailure)
        {
            return new CattleDeviceDetails(IsAvailable: false, CollarId: null, LastReadingReceivedAt: null);
        }

        if (collar.Value is not { } assignment)
        {
            return new CattleDeviceDetails(IsAvailable: true, CollarId: null, LastReadingReceivedAt: null);
        }

        var communication = await operationsMonitoring.GetLastDeviceCommunicationAsync(assignment.DeviceId, cancellationToken);
        return new CattleDeviceDetails(
            IsAvailable: true,
            CollarId: assignment.DeviceId,
            LastReadingReceivedAt: communication?.LastReceivedAt);
    }
}
