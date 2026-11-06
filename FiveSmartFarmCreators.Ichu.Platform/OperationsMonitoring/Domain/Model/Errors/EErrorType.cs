namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;

// Move to Shared when the shared kernel is available.
/// <summary>
///     Classifies a domain error so that each interface can translate it consistently.
/// </summary>
public enum EErrorType
{
    /// <summary>The operation failed for a reason that has no more specific classification.</summary>
    Failure = 0,

    /// <summary>The input does not satisfy a business or format rule.</summary>
    Validation = 1,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 2,

    /// <summary>The operation conflicts with the current state of the resource.</summary>
    Conflict = 3,

    /// <summary>No verified identity is available for the operation.</summary>
    Unauthorized = 4,

    /// <summary>The identity lacks the permission required by the operation.</summary>
    Forbidden = 5,

    /// <summary>The operation failed for an unexpected reason.</summary>
    Unexpected = 6
}
