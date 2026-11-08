using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Attributes a reading through the IoT Assets facade, which knows the assignment that covered the capture instant.
/// </summary>
public class IoTAssetsDeviceAttributionLookup(IIoTAssetsContextFacade iotAssets) : IDeviceAttributionLookup
{
    /// <inheritdoc />
    public async Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken)
    {
        var attribution = await iotAssets.FindDeviceAttributionAsync(deviceId, capturedAt, cancellationToken);
        return attribution is null
            ? null
            : new DeviceAttribution(attribution.DeviceId, attribution.RanchId, attribution.CattleId);
    }
}
