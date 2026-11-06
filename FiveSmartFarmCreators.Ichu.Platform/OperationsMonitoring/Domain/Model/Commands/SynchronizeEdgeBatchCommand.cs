namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Command to synchronize the readings, alerts and alert changes sent by an Edge Gateway.
/// </summary>
/// <param name="Readings">The readings captured by the bands.</param>
/// <param name="Alerts">The alerts evaluated by the gateway.</param>
/// <param name="AlertChanges">The changes to alerts already synchronized.</param>
public sealed record SynchronizeEdgeBatchCommand(
    IReadOnlyList<EdgeReadingEntry> Readings,
    IReadOnlyList<EdgeAlertEntry> Alerts,
    IReadOnlyList<EdgeAlertChangeEntry> AlertChanges);
