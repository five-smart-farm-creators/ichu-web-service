using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application.Ports;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Domain.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Application;

public sealed class SubscriptionAuthorization(ISubscriptionAccessControl accessControl)
{
    public async Task<Guid> RequireAsync(Guid ranchId, SubscriptionPermission permission, CancellationToken cancellationToken)
    {
        var accountId = await accessControl.GetCurrentAccountIdAsync(cancellationToken)
            ?? throw SubscriptionException.Unauthenticated("A verified account is required.");
        if (!await accessControl.CanAccessRanchAsync(accountId, ranchId, permission, cancellationToken))
        {
            throw SubscriptionException.Forbidden("The account is not authorized for the subscription of this ranch.");
        }

        return accountId;
    }
}
