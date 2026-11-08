using FiveSmartFarmCreators.Ichu.Platform.Iam.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Iam.Interfaces.Acl;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Internal.OutboundServices;

/// <summary>
///     Implements <see cref="ISubscriptionAccessService"/> with the IAM context facade, mapping the read and manage
///     permissions to the IAM access scopes that grant them.
/// </summary>
public class IamSubscriptionAccessService(IIamContextFacade iamContextFacade) : ISubscriptionAccessService
{
    /// <inheritdoc />
    public Task<Result<Guid>> EnsureManageAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Manage, cancellationToken);

    /// <inheritdoc />
    public Task<Result<Guid>> EnsureReadAccessAsync(Guid ranchId, CancellationToken cancellationToken) =>
        EnsureAccessAsync(ranchId, AccessScope.Read, cancellationToken);

    private async Task<Result<Guid>> EnsureAccessAsync(Guid ranchId, AccessScope scope, CancellationToken cancellationToken)
    {
        var accountId = await iamContextFacade.FetchCurrentAccountIdAsync(cancellationToken);
        if (accountId is not { } verifiedAccountId)
        {
            return Result<Guid>.Failure(SubscriptionPlansErrors.Unauthenticated);
        }

        return await iamContextFacade.CanAccessRanchAsync(verifiedAccountId, ranchId, scope, cancellationToken)
            ? Result<Guid>.Success(verifiedAccountId)
            : Result<Guid>.Failure(SubscriptionPlansErrors.RanchAccessDenied);
    }
}
