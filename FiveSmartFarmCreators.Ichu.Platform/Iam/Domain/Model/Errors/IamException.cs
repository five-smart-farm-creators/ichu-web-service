namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;

/// <summary>
///     Represents an IAM failure with a stable application error code.
/// </summary>
/// <param name="code">
///     The code used to classify the failure.
/// </param>
/// <param name="message">
///     The message describing the failure.
/// </param>
public sealed class IamException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
