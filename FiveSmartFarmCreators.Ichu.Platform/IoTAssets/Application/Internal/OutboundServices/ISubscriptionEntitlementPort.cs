using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Application.Internal.OutboundServices;

/// <summary>Provides the current paid entitlement; MaximumDevices applies only to assigned collars in IoT Assets.</summary>
public interface ISubscriptionEntitlementPort
{
    Task<Result<CollarEntitlement>> GetAsync(Guid ranchId, CancellationToken cancellationToken);
}
