namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Errors;

/// <summary>
/// Represents invalid Planning data that violates a domain invariant.
/// </summary>
public sealed class PlanningDomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlanningDomainException"/> class.
    /// </summary>
    /// <param name="code">A stable error code for the violated invariant.</param>
    /// <param name="message">A description of the invalid data.</param>
    public PlanningDomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Gets the stable error code for the violated invariant.
    /// </summary>
    public string Code { get; }
}
