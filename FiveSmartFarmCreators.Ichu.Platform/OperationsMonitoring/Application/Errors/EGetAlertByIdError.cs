namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when an alert is retrieved.
/// </summary>
public enum EGetAlertByIdError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The alert does not exist.</summary>
    AlertNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden
}
