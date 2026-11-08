namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

public record SyncEntryResultResource(string Kind, Guid Id, string Status, string? Reason, Guid? RelatedAlertId);
