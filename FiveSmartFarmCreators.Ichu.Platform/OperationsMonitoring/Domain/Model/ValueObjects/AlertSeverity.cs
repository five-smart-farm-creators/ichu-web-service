namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

/// <summary>
///     Ranks how urgent an alert is, as configured by the ranch for the rule that raised it.
/// </summary>
public enum AlertSeverity
{
    /// <summary>The alert only informs the ranch team.</summary>
    Informative,

    /// <summary>The alert requires attention soon.</summary>
    Warning,

    /// <summary>The alert requires immediate attention.</summary>
    Critical
}
