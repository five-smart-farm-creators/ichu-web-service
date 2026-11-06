namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;

/// <summary>Defines the Iam Exception contract.</summary>
public sealed class IamException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
