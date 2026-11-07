namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Errors;

/// <summary>
/// Represents an expected use-case failure that is not a domain validation error.
/// </summary>
public sealed class PlanningRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningRequestException"/> class.
    /// </summary>
    /// <param name="kind">The failure category.</param>
    /// <param name="code">A stable error code.</param>
    /// <param name="message">A safe description of the failure.</param>
    public PlanningRequestException(PlanningFailureKind kind, string code, string message)
        : base(message)
    {
        Kind = kind;
        Code = code;
    }

    /// <summary>
    /// Gets the failure category.
    /// </summary>
    public PlanningFailureKind Kind { get; }

    /// <summary>
    /// Gets the stable error code.
    /// </summary>
    public string Code { get; }
}
