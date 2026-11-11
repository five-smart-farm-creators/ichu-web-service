using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.OutboundServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Application.QueryServices;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.Queries;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Repositories;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.Iam.Application.Internal.QueryServices;

/// <summary>
///     A malformed code gets the same answer as an unknown one, so codes cannot be probed by their format.
/// </summary>
public sealed class RanchQueryService(ICurrentAccount currentAccount, IRanchRegistrationRepository ranches) : IRanchQueryService
{
    /// <inheritdoc />
    public async Task<Result<RanchRegistration>> HandleAsync(GetRanchByAccessCodeQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await currentAccount.GetAsync(cancellationToken);
        if (actor.IsFailure)
        {
            return Result<RanchRegistration>.Failure(actor.DomainError);
        }

        if (actor.Value!.Role != UserRole.Veterinarian)
        {
            return Result<RanchRegistration>.Failure(IamErrors.RanchLookupOnlyForVeterinarians);
        }

        var accessCode = RanchAccessCode.Create(query.AccessCode);
        if (accessCode.IsFailure)
        {
            return Result<RanchRegistration>.Failure(IamErrors.RanchNotFound);
        }

        var ranch = await ranches.FindByAccessCodeAsNoTrackingAsync(accessCode.Value!, cancellationToken);
        return ranch is null
            ? Result<RanchRegistration>.Failure(IamErrors.RanchNotFound)
            : Result<RanchRegistration>.Success(ranch);
    }
}
