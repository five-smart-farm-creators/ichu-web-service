using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Transform;

/// <summary>
///     Assembles HTTP response resources from the results of a synchronized batch.
/// </summary>
public static class SyncBatchResultResourceFromEntityAssembler
{
    /// <summary>
    ///     Transforms the individual results of a batch into a <see cref="SyncBatchResultResource"/>.
    /// </summary>
    /// <param name="results">The results, one per entry in processing order.</param>
    /// <returns>A resource representation suitable for HTTP responses.</returns>
    public static SyncBatchResultResource ToResourceFromEntity(IReadOnlyList<SyncEntryResult> results) =>
        new(results.Select(ToEntryResource).ToList());

    private static SyncEntryResultResource ToEntryResource(SyncEntryResult result) =>
        new(result.Kind.ToString(), result.Id, result.Status.ToString(), result.Reason, result.RelatedAlertId);
}
