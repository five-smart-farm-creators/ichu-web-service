using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Resources;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Builds an account-registration command from REST input.
/// </summary>
public static class RegisterAccountCommandFromResourceAssembler
{
    /// <summary>
    ///     Parses the requested role and copies optional ranch details into a command.
    /// </summary>
    /// <param name="resource">
    ///     The validated account-registration request.
    /// </param>
    /// <returns>
    ///     The command to register the account and optional ranch.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     The request is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     The requested role is not defined.
    /// </exception>
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
