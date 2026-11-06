namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource representing the individual result of a synchronized entry.
/// </summary>
/// <param name="Kind">The kind of entry.</param>
/// <param name="Id">The identifier the client generated for the entry.</param>
/// <param name="Status">The outcome: Accepted, Duplicate, Rejected or Conflict.</param>
/// <param name="Reason">The reason for the outcome, when it needs one.</param>
/// <param name="RelatedAlertId">The stored alert the entry belongs to, when it joined or changed an alert.</param>
public record SyncEntryResultResource(string Kind, Guid Id, string Status, string? Reason, Guid? RelatedAlertId);
