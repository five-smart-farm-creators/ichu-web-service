namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Identifies the kind of operation applied to an existing alert.
/// </summary>
public enum AlertChangeKind
{
    /// <summary>New readings support the alert condition.</summary>
    Evidence,

    /// <summary>The animal returned to the permitted area.</summary>
    Cleared,

    /// <summary>An authorized user recorded attention for the alert.</summary>
    Acknowledged
}
