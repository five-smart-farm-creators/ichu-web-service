namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

/// <summary>
///     Classifies a domain error so that each interface can translate it to its own failure representation.
/// </summary>
public enum EErrorType
{
    /// <summary>A failure that does not fit any other classification.</summary>
    Failure = 0,

    /// <summary>The input breaks a validation rule.</summary>
    Validation = 1,

    /// <summary>The requested resource does not exist.</summary>
    NotFound = 2,

    /// <summary>The operation conflicts with the current state.</summary>
    Conflict = 3,

    /// <summary>No authenticated caller is available.</summary>
    Unauthorized = 4,

    /// <summary>The caller lacks the required permission.</summary>
    Forbidden = 5,

    /// <summary>An unexpected technical failure occurred.</summary>
    Unexpected = 6
}
