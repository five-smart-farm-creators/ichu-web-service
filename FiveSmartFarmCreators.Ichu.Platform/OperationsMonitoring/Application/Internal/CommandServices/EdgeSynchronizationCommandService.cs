using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Synchronizes the batches sent by Edge Gateways. Readings are processed first, so that an alert in the same batch can cite them as evidence.
/// </summary>
public class EdgeSynchronizationCommandService(
    IEdgeGatewayAuthenticator gatewayAuthenticator,
    ITelemetryReadingCommandService readingCommandService,
    IMonitoringAlertCommandService alertCommandService,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IEdgeSynchronizationCommandService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SyncEntryResult>>> Handle(SynchronizeEdgeBatchCommand command, CancellationToken cancellationToken)
    {
        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.GatewayUnauthenticated);
        }

        var entryCount = command.Readings.Count + command.Alerts.Count + command.AlertChanges.Count;
        if (entryCount == 0 || entryCount > options.Value.MaximumBatchEntries)
        {
            return Result<IReadOnlyList<SyncEntryResult>>.Failure(OperationsMonitoringErrors.InvalidEdgeBatchSize);
        }

        var receivedAt = clock.GetUtcNow();
        var results = new List<SyncEntryResult>(entryCount);

        foreach (var entry in command.Readings)
        {
            results.Add(await readingCommandService.Handle(new SynchronizeReadingCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt), cancellationToken));
        }

        foreach (var entry in command.Alerts)
        {
            results.Add(await alertCommandService.Handle(new SynchronizeAlertCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt), cancellationToken));
        }

        foreach (var entry in command.AlertChanges)
        {
            results.Add(await alertCommandService.Handle(new SynchronizeAlertChangeCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt), cancellationToken));
        }

        return Result<IReadOnlyList<SyncEntryResult>>.Success(results);
    }
}
