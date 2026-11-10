using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.CommandServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Commands;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Repositories;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.CommandServices;

/// <summary>
///     Coordinates professional profile completion. The license is checked against other accounts before it is assigned.
/// </summary>
public sealed class ProfessionalProfileCommandService(
    IUserAccountRepository accounts,
    ICurrentAccount currentAccount,
    IUnitOfWork unitOfWork) : IProfessionalProfileCommandService
{
    /// <inheritdoc />
    public async Task<Result<UserAccount>> HandleAsync(CompleteProfessionalProfileCommand command, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return actor;
        }

        var profile = ProfessionalProfile.Create(command.Specialty, command.LicenseNumber, command.YearsOfExperience);
        if (profile.IsFailure)
        {
            return Result<UserAccount>.Failure(profile.DomainError);
        }

        var account = actor.Value!;
        if (await accounts.LicenseExistsAsync(profile.Value!.LicenseNumber, account.Id, cancellationToken))
        {
            return Result<UserAccount>.Failure(IamErrors.LicenseAlreadyRegistered);
        }

        var completion = account.CompleteProfile(profile.Value!);
        if (completion.IsFailure)
        {
            return Result<UserAccount>.Failure(completion.DomainError);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return actor;
    }
}
