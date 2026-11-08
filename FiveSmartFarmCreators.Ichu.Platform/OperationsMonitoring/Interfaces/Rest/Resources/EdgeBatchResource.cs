namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record EdgeBatchResource(
    IReadOnlyList<EdgeReadingResource>? Readings,
    IReadOnlyList<EdgeAlertResource>? Alerts,
    IReadOnlyList<EdgeAlertChangeResource>? AlertChanges);
