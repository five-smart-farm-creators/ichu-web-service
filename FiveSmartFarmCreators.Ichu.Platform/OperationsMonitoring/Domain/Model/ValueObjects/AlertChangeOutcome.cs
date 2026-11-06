namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Describes what happened when an operation was applied to an alert.
/// </summary>
public enum AlertChangeOutcome
{
    /// <summary>The operation changed the alert.</summary>
    Applied,

    /// <summary>The same operation had already been applied, so nothing changed.</summary>
    AlreadyApplied,

    /// <summary>The operation clashes with the stored state or with a different operation.</summary>
    Conflict
}
