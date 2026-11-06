using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>Coordinates professional profile completion.</summary>
public sealed class ProfessionalProfileCommandService(IIamRepository repository, ICurrentAccount currentAccount)
    : IProfessionalProfileCommandService
{
    /// <inheritdoc />
    public async Task<UserAccount> HandleAsync(CompleteProfessionalProfileCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        var profile = new ProfessionalProfile(command.Specialty, command.LicenseNumber, command.YearsOfExperience);
        if (await repository.LicenseExistsAsync(profile.LicenseNumber, actor.Id, cancellationToken))
            throw new IamException("conflict", "The professional license is already registered.");
        actor.CompleteProfile(profile);
        await repository.SaveAsync(cancellationToken);
        return actor;
    }
}
