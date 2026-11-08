using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

public static class SyncBatchResultResourceFromEntityAssembler
{
    public static SyncBatchResultResource ToResourceFromEntity(IReadOnlyList<SyncEntryResult> results) =>
        new(results.Select(ToEntryResource).ToList());

    private static SyncEntryResultResource ToEntryResource(SyncEntryResult result) =>
        new(result.Kind.ToString(), result.Id, result.Status.ToString(), result.Reason, result.RelatedAlertId?.Value);
}
