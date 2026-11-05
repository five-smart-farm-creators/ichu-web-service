namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

public sealed record SyncEntryResult(SyncEntryKind Kind, Guid Id, SyncEntryStatus Status, string? Reason, Guid? RelatedAlertId = null);
