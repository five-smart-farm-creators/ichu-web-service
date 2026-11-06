namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;

/// <summary>Identifies a domain failure with a stable error code.</summary>
public sealed class IamException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
