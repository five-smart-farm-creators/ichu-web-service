using System.Globalization;
using System.Resources;
using FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Resources;
using FiveSmartFarmCreators.Ichu.Platform.Shared.Domain.Model.Errors;

namespace FiveSmartFarmCreators.Ichu.Platform.IoTAssets.Domain.Model.Errors;

/// <summary>
///     Business errors of the IoT Assets context. Each code is also the resource key of its localized message, so the
///     English text lives in one place.
/// </summary>
public static class IoTAssetsErrors
{
    // Declared first because the error fields below read their messages during static initialization.
    private static readonly ResourceManager Messages = new(typeof(IoTAssetsMessages));

    public static readonly Error InvalidInput = Define("InvalidInput", EErrorType.Validation);
    public static readonly Error DeviceNotFound = Define("DeviceNotFound", EErrorType.NotFound);
    public static readonly Error WaterTroughNotFound = Define("WaterTroughNotFound", EErrorType.NotFound);
    public static readonly Error TargetNotFound = Define("TargetNotFound", EErrorType.NotFound);
    public static readonly Error InvalidState = Define("InvalidState", EErrorType.Conflict);
    public static readonly Error DuplicateSerial = Define("DuplicateSerial", EErrorType.Conflict);
    public static readonly Error TargetAlreadyAssigned = Define("TargetAlreadyAssigned", EErrorType.Conflict);
    public static readonly Error DeviceAlreadyAssigned = Define("DeviceAlreadyAssigned", EErrorType.Conflict);
    public static readonly Error DeviceLimitReached = Define("DeviceLimitReached", EErrorType.Conflict);
    public static readonly Error SubscriptionRequired = Define("SubscriptionRequired", EErrorType.Forbidden);

    /// <summary>The data changed after the caller read it, so the caller must reload it before retrying.</summary>
    public static readonly Error Conflict = Define("Conflict", EErrorType.Conflict);

    public static readonly Error Unauthenticated = Define("Unauthenticated", EErrorType.Unauthorized);
    public static readonly Error Forbidden = Define("Forbidden", EErrorType.Forbidden);

    /// <summary>A transaction could not run safely, so the caller may retry once the integration recovers.</summary>
    public static readonly Error IntegrationUnavailable = Define("IntegrationUnavailable", EErrorType.Failure);

    private static Error Define(string name, EErrorType type)
    {
        var code = $"IoTAssets.{name}";
        return new Error(code, Messages.GetString(code, CultureInfo.InvariantCulture) ?? code, type);
    }
}
