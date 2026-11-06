namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when a geofence is defined.
/// </summary>
public enum EDefineGeofenceError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden,

    /// <summary>The lot does not exist in the ranch.</summary>
    LotNotFound,

    /// <summary>The lot already has a geofence.</summary>
    GeofenceAlreadyDefined
}
