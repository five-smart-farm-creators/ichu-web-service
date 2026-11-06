using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>Transforms IAM boundary representations without invoking business operations.</summary>
public static class RegisterAccountCommandFromResourceAssembler
{
    /// <summary>Maps the supplied representation to the target contract.</summary>
    public static RegisterAccountCommand ToCommandFromResource(RegisterAccountResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        if (!Enum.TryParse<UserRole>(resource.Role, true, out var role) || !Enum.IsDefined(role))
        {
            throw new ArgumentException("The requested role is invalid.");
        }
        return new RegisterAccountCommand(role, resource.RanchName, resource.Region, resource.District, resource.EstimatedHeadCount);
    }
}
