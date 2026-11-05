using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record SyncBatchResultResource(IReadOnlyList<SyncEntryResultResource> Results)
{
    public static SyncBatchResultResource From(IReadOnlyList<SyncEntryResult> results) =>
        new(results.Select(SyncEntryResultResource.From).ToList());
}
