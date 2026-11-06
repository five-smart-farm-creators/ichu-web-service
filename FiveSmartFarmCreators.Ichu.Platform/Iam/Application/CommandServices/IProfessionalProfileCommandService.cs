using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>Defines the ProfessionalProfileCommandService application contract.</summary>
public interface IProfessionalProfileCommandService
{
    /// <summary>Completes the current veterinarian profile while enforcing license uniqueness.</summary>
    Task<UserAccount> HandleAsync(CompleteProfessionalProfileCommand command, CancellationToken cancellationToken = default);
}
