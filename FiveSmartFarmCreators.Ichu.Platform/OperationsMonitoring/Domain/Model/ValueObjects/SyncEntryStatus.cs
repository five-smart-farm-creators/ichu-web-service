namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Describes the individual result of a synchronized entry. Clients mark only accepted and duplicate entries as
///     synchronized; rejected and conflicting entries are kept with their reason for review.
/// </summary>
public enum SyncEntryStatus
{
    /// <summary>The entry was stored.</summary>
    Accepted,

    /// <summary>The entry had already been stored, so nothing changed.</summary>
    Duplicate,

    /// <summary>The entry is invalid or not allowed and was not stored.</summary>
    Rejected,

    /// <summary>The entry clashes with a different stored entry that uses the same identifier.</summary>
    Conflict
}
