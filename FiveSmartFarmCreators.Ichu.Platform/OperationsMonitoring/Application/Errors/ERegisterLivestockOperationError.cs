namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when a livestock operation is registered.
/// </summary>
public enum ERegisterLivestockOperationError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The animal does not exist.</summary>
    AnimalNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden,

    /// <summary>The event time is ahead of the server clock beyond the configured tolerance.</summary>
    EventTimeAhead,

    /// <summary>The animal was not active in the herd when the operation occurred.</summary>
    AnimalNotActive,

    /// <summary>A different operation already uses the identifier.</summary>
    OperationConflict
}
