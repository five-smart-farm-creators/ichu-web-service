using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Configuration;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Infrastructure.Simulation.Services;

/// <summary>
///     Development-only implementation of <see cref="ICattleLookup"/> that reads the animals and lots of the
///     "OperationsMonitoring:Simulation" configuration section.
/// </summary>
/// <param name="options">The simulated data.</param>
public class SimulatedCattleLookup(IOptions<MonitoringSimulationOptions> options) : ICattleLookup
{
    /// <inheritdoc />
    public Task<CattleSnapshot?> FindAsync(Guid cattleId, CancellationToken cancellationToken)
    {
        var cattle = options.Value.Cattle.FirstOrDefault(c => c.CattleId == cattleId);
        return Task.FromResult<CattleSnapshot?>(cattle is null
            ? null
            : new CattleSnapshot(cattle.CattleId, cattle.RanchId, cattle.IncorporatedAt, cattle.DeactivatedAt));
    }

    /// <inheritdoc />
    public Task<bool> LotBelongsToRanchAsync(Guid lotId, Guid ranchId, CancellationToken cancellationToken)
    {
        return Task.FromResult(options.Value.Lots.Any(l => l.LotId == lotId && l.RanchId == ranchId));
    }
}
