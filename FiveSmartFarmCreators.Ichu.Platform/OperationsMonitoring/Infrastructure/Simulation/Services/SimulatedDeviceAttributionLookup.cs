using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Services;

/// <summary>
///     Development-only implementation of <see cref="IDeviceAttributionLookup"/> that reads the collars of the
///     "OperationsMonitoring:Simulation" configuration section. A reading captured before the assignment instant is
///     attributed to the ranch only.
/// </summary>
/// <param name="options">The simulated data.</param>
public class SimulatedDeviceAttributionLookup(IOptions<MonitoringSimulationOptions> options) : IDeviceAttributionLookup
{
    /// <inheritdoc />
    public Task<DeviceAttribution?> FindAsync(Guid deviceId, DateTimeOffset capturedAt, CancellationToken cancellationToken)
    {
        var device = options.Value.Devices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device is null)
        {
            return Task.FromResult<DeviceAttribution?>(null);
        }

        var cattleId = capturedAt >= device.AssignedAt ? device.CattleId : null;
        return Task.FromResult<DeviceAttribution?>(new DeviceAttribution(device.DeviceId, device.RanchId, cattleId));
    }
}
