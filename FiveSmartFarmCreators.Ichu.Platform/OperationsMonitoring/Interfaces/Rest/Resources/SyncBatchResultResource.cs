namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record SyncBatchResultResource(IReadOnlyList<SyncEntryResultResource> Results);
