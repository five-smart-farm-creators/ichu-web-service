namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Describes whether an alert has received attention.
/// </summary>
public enum AlertStatus
{
    /// <summary>No authorized user has attended the alert yet.</summary>
    Pending,

    /// <summary>An authorized user recorded a finding for the alert.</summary>
    Acknowledged
}
