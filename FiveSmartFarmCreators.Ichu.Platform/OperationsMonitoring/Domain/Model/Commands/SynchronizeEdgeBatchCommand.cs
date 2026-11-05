namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

public sealed record SynchronizeEdgeBatchCommand(
    IReadOnlyList<EdgeReadingEntry> Readings,
    IReadOnlyList<EdgeAlertEntry> Alerts,
    IReadOnlyList<EdgeAlertChangeEntry> AlertChanges);
