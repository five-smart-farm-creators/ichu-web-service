using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.OutboundServices.ACL;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Patterns;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;
using Microsoft.Extensions.Options;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Internal.CommandServices;

/// <summary>
///     Application command service that implements <see cref="IEdgeSynchronizationCommandService"/> to synchronize the
///     batches sent by Edge Gateways.
/// </summary>
/// <param name="gatewayAuthenticator">The port that verifies the gateway credentials.</param>
/// <param name="readingCommandService">The command service that synchronizes readings.</param>
/// <param name="alertCommandService">The command service that synchronizes alerts and alert changes.</param>
/// <param name="options">The technical limits of the module.</param>
/// <param name="clock">The clock that provides the reception time.</param>
public class EdgeSynchronizationCommandService(
    IEdgeGatewayAuthenticator gatewayAuthenticator,
    ITelemetryReadingCommandService readingCommandService,
    IMonitoringAlertCommandService alertCommandService,
    IOptions<OperationsMonitoringOptions> options,
    TimeProvider clock) : IEdgeSynchronizationCommandService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>> Handle(SynchronizeEdgeBatchCommand command, CancellationToken cancellationToken)
    {
        if (await gatewayAuthenticator.AuthenticateAsync(cancellationToken) is not { } gateway)
        {
            return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>.Failure(ESynchronizeEdgeBatchError.Unauthenticated);
        }

        var entryCount = command.Readings.Count + command.Alerts.Count + command.AlertChanges.Count;
        if (entryCount == 0 || entryCount > options.Value.MaximumBatchEntries)
        {
            return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>.Failure(ESynchronizeEdgeBatchError.InvalidBatchSize);
        }

        var receivedAt = clock.GetUtcNow();
        var results = new List<SyncEntryResult>(entryCount);

        // Readings are processed first so that alerts in the same batch can reference them as evidence.
        foreach (var entry in command.Readings)
        {
            var reading = new SynchronizeReadingCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt);
            results.Add(await readingCommandService.Handle(reading, cancellationToken));
        }

        foreach (var entry in command.Alerts)
        {
            var alert = new SynchronizeAlertCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt);
            results.Add(await alertCommandService.Handle(alert, cancellationToken));
        }

        foreach (var entry in command.AlertChanges)
        {
            var change = new SynchronizeAlertChangeCommand(gateway.GatewayId, gateway.RanchId, entry, receivedAt);
            results.Add(await alertCommandService.Handle(change, cancellationToken));
        }

        return new Result<IReadOnlyList<SyncEntryResult>, ESynchronizeEdgeBatchError>.Success(results);
    }
}
