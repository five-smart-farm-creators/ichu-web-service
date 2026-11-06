using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;

/// <summary>
///     Handles veterinarian professional-profile completion.
/// </summary>
public interface IProfessionalProfileCommandService
{
    /// <summary>
    ///     Completes the current veterinarian profile while enforcing license uniqueness.
    /// </summary>
    /// <returns>
    ///     The account with its completed professional profile.
    /// </returns>
    Task<UserAccount> HandleAsync(CompleteProfessionalProfileCommand command, CancellationToken cancellationToken = default);
}
