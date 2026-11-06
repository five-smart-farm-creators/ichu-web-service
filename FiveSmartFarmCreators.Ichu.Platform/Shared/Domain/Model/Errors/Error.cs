namespace FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

/// <summary>
///     Represents a domain error with a stable code, a message and a classification.
/// </summary>
/// <param name="Code">The unique error code, prefixed by the bounded context.</param>
/// <param name="Message">The error message.</param>
/// <param name="Type">The error classification.</param>
public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure)
{
    /// <summary>
    ///     Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    ///     Represents a result whose value is unexpectedly missing.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.");

    /// <summary>
    ///     Creates an error for a resource that does not exist.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);

    /// <summary>
    ///     Creates an error for an operation that conflicts with the current state.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);

    /// <summary>
    ///     Creates an error for input that breaks a validation rule.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);

    /// <summary>
    ///     Creates an error that does not fit any other classification.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Failure(string code, string message) => new(code, message);

    /// <summary>
    ///     Creates an error for a request without an authenticated caller.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);

    /// <summary>
    ///     Creates an error for a caller that lacks the required permission.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Forbidden(string code, string message) => new(code, message, EErrorType.Forbidden);

    /// <summary>
    ///     Creates an error for an unexpected technical failure.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error.</returns>
    public static Error Unexpected(string code, string message) => new(code, message, EErrorType.Unexpected);
}
