using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

public static class IoTAssetsErrors
{
    public static readonly Error InvalidInput = Error.Validation("IoTAssets.InvalidInput", "The supplied device data is invalid.");
    public static readonly Error DeviceNotFound = Error.NotFound("IoTAssets.DeviceNotFound", "The device was not found in the authorized ranch.");
    public static readonly Error WaterTroughNotFound = Error.NotFound("IoTAssets.WaterTroughNotFound", "The water trough was not found in the authorized ranch.");
    public static readonly Error TargetNotFound = Error.NotFound("IoTAssets.TargetNotFound", "The assignment target was not found in the authorized ranch.");
    public static readonly Error InvalidState = Error.Conflict("IoTAssets.InvalidState", "The current state does not allow this operation.");
    public static readonly Error DuplicateSerial = Error.Conflict("IoTAssets.DuplicateSerial", "The device serial is already registered.");
    public static readonly Error TargetAlreadyAssigned = Error.Conflict("IoTAssets.TargetAlreadyAssigned", "The target already has an assigned device.");
    public static readonly Error DeviceAlreadyAssigned = Error.Conflict("IoTAssets.DeviceAlreadyAssigned", "The device already has an open assignment.");
    public static readonly Error DeviceLimitReached = Error.Conflict("IoTAssets.DeviceLimitReached", "The assigned collar limit of the current plan has been reached.");
    public static readonly Error SubscriptionRequired = Error.Forbidden("IoTAssets.SubscriptionRequired", "An active subscription is required to assign a collar.");
    public static readonly Error Conflict = Error.Conflict("IoTAssets.Conflict", "The data changed concurrently. Reload it before retrying.");
    public static readonly Error Unauthenticated = Error.Unauthorized("IoTAssets.Unauthenticated", "An authenticated caller is required.");
    public static readonly Error Forbidden = Error.Forbidden("IoTAssets.Forbidden", "The caller does not have the required ranch permission.");
    public static readonly Error IntegrationUnavailable = Error.Failure("IoTAssets.IntegrationUnavailable", "A required integration is unavailable.");
}
