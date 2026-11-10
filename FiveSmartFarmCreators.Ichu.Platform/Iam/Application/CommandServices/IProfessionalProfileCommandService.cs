using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

public interface IProfessionalProfileCommandService
{
    Task<Result<UserAccount>> HandleAsync(CompleteProfessionalProfileCommand command, CancellationToken cancellationToken = default);
}
