using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>
///     Handles account registration and sign-in commands.
/// </summary>
public interface IAccountCommandService
{
    /// <summary>
    ///     Registers an account and its optional owner membership.
    /// </summary>
    /// <param name="command">
    ///     The account role and optional ranch details.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels account registration and persistence.
    /// </param>
    /// <returns>
    ///     The registered account and optional ranch identifier.
    /// </returns>
    Task<RegisteredAccount> HandleAsync(RegisterAccountCommand command, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Authenticates credentials through the identity provider and applies account lockout.
    /// </summary>
    /// <param name="command">
    ///     The credentials submitted for sign-in.
    /// </param>
    /// <param name="cancellationToken">
    ///     Cancels credential authentication.
    /// </param>
    /// <returns>
    ///     The identity token issued by the provider.
    /// </returns>
    Task<string> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default);
}
