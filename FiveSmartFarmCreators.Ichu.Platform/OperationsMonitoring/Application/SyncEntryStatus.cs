namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application;

/// <summary>
/// The individual result of a synchronized entry. Clients mark only accepted and duplicate entries as
/// synchronized; rejected and conflicting entries are kept with their reason for review.
/// </summary>
public enum SyncEntryStatus
{
    Accepted,
    Duplicate,
    Rejected,
    Conflict
}
