using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Aggregates;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Entities;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.ValueObjects;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Application.Model;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Services;

/// <summary>
///     Applies the collar rules that depend on the ranch subscription before a device starts serving a target.
/// </summary>
public class DeviceAssignmentDomainService
{
    public Result<DeviceAssignment> Assign(IoTDevice device, Guid targetId, int assignedCollars,
        int maximumCollars, bool subscriptionActive, DateTimeOffset at)
    {
        if (device.Type == DeviceType.SmartCollar)
        {
            if (!subscriptionActive)
            {
                return Result<DeviceAssignment>.Failure(IoTAssetsErrors.SubscriptionRequired);
            }

            if (assignedCollars < 0 || maximumCollars < 0)
            {
                return Result<DeviceAssignment>.Failure(IoTAssetsErrors.InvalidInput);
            }

            if (assignedCollars >= maximumCollars)
            {
                return Result<DeviceAssignment>.Failure(IoTAssetsErrors.DeviceLimitReached);
            }
        }

        return device.AssignTo(targetId, at);
    }
}
