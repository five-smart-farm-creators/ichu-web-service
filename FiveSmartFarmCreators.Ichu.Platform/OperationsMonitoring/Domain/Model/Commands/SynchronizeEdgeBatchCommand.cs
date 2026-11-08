namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Synchronizes the readings, alerts and alert changes sent by an Edge Gateway.
/// </summary>
public record SynchronizeEdgeBatchCommand(
    IReadOnlyList<EdgeReadingEntry> Readings,
    IReadOnlyList<EdgeAlertEntry> Alerts,
    IReadOnlyList<EdgeAlertChangeEntry> AlertChanges);
