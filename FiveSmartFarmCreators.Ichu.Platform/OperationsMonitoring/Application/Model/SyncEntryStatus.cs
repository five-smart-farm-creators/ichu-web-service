namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Clients mark only accepted and duplicate entries as synchronized; rejected and conflicting entries are kept with their reason for review.
/// </summary>
public enum SyncEntryStatus { Accepted, Duplicate, Rejected, Conflict }
