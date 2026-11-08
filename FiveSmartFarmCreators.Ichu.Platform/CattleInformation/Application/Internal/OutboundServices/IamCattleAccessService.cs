using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Application.Internal.OutboundServices;

/// <summary>
///     Implements <see cref="ICattleAccessService"/> with the IAM context facade, mapping the read and manage checks to
///     the IAM access scopes that grant them.
/// </summary>
public class IamCattleAccessService(IIamContextFacade iamContextFacade) : ICattleAccessService
{
    /// <inheritdoc />
    public Task<Result> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Read, cancellationToken);

    /// <inheritdoc />
    public Task<Result> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Manage, cancellationToken);

    private async Task<Result> EnsureAccessAsync(Guid ranchId, AccessScope scope, CancellationToken cancellationToken)
    {
        var accountId = await iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId)
        {
            return Result.Failure(CattleInformationErrors.Unauthenticated);
        }

        return await iamContextFacade.CanAccessRanchAsync(verifiedAccountId, ranchId, scope, cancellationToken)
            ? Result.Success()
            : Result.Failure(CattleInformationErrors.RanchAccessDenied);
    }
}
