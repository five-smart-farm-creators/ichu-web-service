namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Ports;

/// <summary>Anti-corruption port towards IoT Assets.</summary>
public interface IDeviceAttributionLookup
{
    /// <summary>
    /// Returns the device owner and the assignment that was effective at the capture instant, or null when the
    /// device is not registered. Later releases or reassignments must not change the historical result.
    /// </summary>
    Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken);
}
