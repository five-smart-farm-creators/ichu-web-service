using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

public interface IAccountCommandService
{
    Task<Result<RegisteredAccount>> HandleAsync(RegisterAccountCommand command, CancellationToken cancellationToken = default);

    Task<Result<string>> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default);
}
