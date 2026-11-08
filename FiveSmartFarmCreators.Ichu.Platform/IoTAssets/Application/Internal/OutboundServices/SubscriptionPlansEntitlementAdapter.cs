using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;
using FiveSmartFarmCreators.Ichu.Platform.SubscriptionPlans.Interfaces.Acl;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>
///     Turns the subscription entitlements of a ranch into the collar limit that IoT Assets enforces on assignments.
/// </summary>
public class SubscriptionPlansEntitlementAdapter(ISubscriptionPlansContextFacade subscriptionPlans) : ISubscriptionEntitlementPort
{
    /// <inheritdoc />
    public async Task<Result<CollarEntitlement>> GetAsync(Guid ranchId, CancellationToken cancellationToken)
    {
        var entitlements = await subscriptionPlans.GetEntitlementsAsync(ranchId, cancellationToken);
        return Result<CollarEntitlement>.Success(new CollarEntitlement(entitlements.IsActive, entitlements.MaximumDevices));
    }
}
