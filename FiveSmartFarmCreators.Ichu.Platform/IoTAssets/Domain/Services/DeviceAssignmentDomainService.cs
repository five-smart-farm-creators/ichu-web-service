using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;

public class DeviceAssignmentDomainService
{
    public DeviceAssignment Assign(IoTDevice device, Guid targetId, int assignedCollars,
        int maximumCollars, bool subscriptionActive, DateTimeOffset at)
    {
        if (device.Type == DeviceType.SmartCollar)
        {
            if (!subscriptionActive)
            {
                throw new IoTAssetsException(IoTAssetsErrors.SubscriptionRequired);
            }

            if (assignedCollars < 0 || maximumCollars < 0)
            {
                throw new IoTAssetsException(IoTAssetsErrors.InvalidInput);
            }

            if (assignedCollars >= maximumCollars)
            {
                throw new IoTAssetsException(IoTAssetsErrors.DeviceLimitReached);
            }
        }

        return device.AssignTo(targetId, at);
    }
}
