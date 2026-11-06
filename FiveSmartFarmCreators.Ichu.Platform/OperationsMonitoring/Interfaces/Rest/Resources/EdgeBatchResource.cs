namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the batch that an Edge Gateway synchronizes.
/// </summary>
/// <param name="Readings">The readings captured by the bands.</param>
/// <param name="Alerts">The alerts evaluated by the gateway.</param>
/// <param name="AlertChanges">The changes to alerts already synchronized.</param>
public record EdgeBatchResource(
    IReadOnlyList<EdgeReadingResource>? Readings,
    IReadOnlyList<EdgeAlertResource>? Alerts,
    IReadOnlyList<EdgeAlertChangeResource>? AlertChanges);
