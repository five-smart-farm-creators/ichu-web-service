using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public sealed record SyncEntryResultResource(string Kind, Guid Id, string Status, string? Reason, Guid? RelatedAlertId)
{
    public static SyncEntryResultResource From(SyncEntryResult result) =>
        new(result.Kind.ToString(), result.Id, result.Status.ToString(), result.Reason, result.RelatedAlertId);
}
