namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;

/// <summary>
///     Anti-corruption port towards IoT Assets.
/// </summary>
public interface IDeviceAttributionLookup
{
    /// <summary>
    ///     Finds the device owner and the assignment that was effective at the capture instant. Later releases or reassignments must not change that result.
    /// </summary>
    Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken);
}
