namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;

/// <summary>
///     Anti-corruption port towards IoT Assets.
/// </summary>
public interface IDeviceAttributionLookup
{
    /// <summary>
    ///     Finds the device owner and the assignment that was effective at the capture instant asynchronously. Later
    ///     releases or reassignments must not change the historical result.
    /// </summary>
    /// <param name="deviceId">The device identifier.</param>
    /// <param name="capturedAt">The instant the reading was captured.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A task containing the attribution, or <see langword="null"/> when the device is not registered.</returns>
    Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken);
}
