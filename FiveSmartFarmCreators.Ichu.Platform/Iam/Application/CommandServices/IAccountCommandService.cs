using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>Defines the AccountCommandService application contract.</summary>
public interface IAccountCommandService
{
    /// <summary>Registers an account and its optional owner membership.</summary>
    Task<RegisteredAccount> HandleAsync(RegisterAccountCommand command, CancellationToken cancellationToken = default);

    /// <summary>Authenticates credentials through the identity provider and applies account lockout.</summary>
    Task<string> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default);
}
