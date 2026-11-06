namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Value object representing the individual result of one synchronized entry.
/// </summary>
/// <param name="Kind">The kind of entry.</param>
/// <param name="Id">The identifier the client generated for the entry.</param>
/// <param name="Status">The outcome of the entry.</param>
/// <param name="Reason">The reason for the outcome, when it needs one.</param>
/// <param name="RelatedAlertId">The stored alert the entry belongs to, when it joined or changed an alert.</param>
public sealed record SyncEntryResult(SyncEntryKind Kind, Guid Id, SyncEntryStatus Status, string? Reason, Guid? RelatedAlertId = null);
