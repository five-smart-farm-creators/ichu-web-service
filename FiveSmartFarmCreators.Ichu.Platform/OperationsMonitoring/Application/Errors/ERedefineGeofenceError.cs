namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when a geofence is redefined.
/// </summary>
public enum ERedefineGeofenceError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The geofence does not exist.</summary>
    GeofenceNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden
}
