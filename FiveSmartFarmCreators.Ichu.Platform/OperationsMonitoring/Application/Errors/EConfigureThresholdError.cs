namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when a threshold is configured.
/// </summary>
public enum EConfigureThresholdError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden,

    /// <summary>The rules changed concurrently; reload them before retrying.</summary>
    ConcurrentModification
}
