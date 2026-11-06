namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Errors;

// Move to Shared when the shared kernel is available.
/// <summary>
///     Represents a domain error.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Message">The error message.</param>
/// <param name="Type">The error classification type.</param>
public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure)
{
    /// <summary>Represents the absence of an error.</summary>
    public static readonly Error None = new(string.Empty, string.Empty, EErrorType.Failure);

    /// <summary>Represents a result value that is unexpectedly null.</summary>
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", EErrorType.Failure);

    /// <summary>Creates an error for a resource that does not exist.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.NotFound"/>.</returns>
    public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);

    /// <summary>Creates an error for an operation that conflicts with the current state.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Conflict"/>.</returns>
    public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);

    /// <summary>Creates an error for an input that violates a rule.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Validation"/>.</returns>
    public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);

    /// <summary>Creates an error for an operation that failed without a more specific classification.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Failure"/>.</returns>
    public static Error Failure(string code, string message) => new(code, message, EErrorType.Failure);

    /// <summary>Creates an error for an operation without a verified identity.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Unauthorized"/>.</returns>
    public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);

    /// <summary>Creates an error for an identity that lacks the required permission.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Forbidden"/>.</returns>
    public static Error Forbidden(string code, string message) => new(code, message, EErrorType.Forbidden);

    /// <summary>Creates an error for an operation that failed unexpectedly.</summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <returns>The error classified as <see cref="EErrorType.Unexpected"/>.</returns>
    public static Error Unexpected(string code, string message) => new(code, message, EErrorType.Unexpected);
}
