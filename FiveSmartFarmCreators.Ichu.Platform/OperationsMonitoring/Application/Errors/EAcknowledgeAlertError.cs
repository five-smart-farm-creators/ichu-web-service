namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when an alert is acknowledged.
/// </summary>
public enum EAcknowledgeAlertError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The alert does not exist.</summary>
    AlertNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden,

    /// <summary>The alert version is stale, or a different operation already attended the alert.</summary>
    Conflict
}
