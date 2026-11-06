namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Model;

/// <summary>
///     Identifies the kind of entry that a client synchronizes.
/// </summary>
public enum SyncEntryKind
{
    /// <summary>A telemetry reading relayed by an Edge Gateway.</summary>
    Reading,

    /// <summary>An alert evaluated by an Edge Gateway.</summary>
    Alert,

    /// <summary>A change to an alert already synchronized.</summary>
    AlertChange,

    /// <summary>A livestock operation recorded offline by the mobile application.</summary>
    LivestockOperation
}
