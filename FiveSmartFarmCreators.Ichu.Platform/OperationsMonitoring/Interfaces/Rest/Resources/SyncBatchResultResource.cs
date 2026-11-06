namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the results of a synchronized batch, one per entry in processing order.
/// </summary>
/// <param name="Results">The individual results.</param>
public record SyncBatchResultResource(IReadOnlyList<SyncEntryResultResource> Results);
