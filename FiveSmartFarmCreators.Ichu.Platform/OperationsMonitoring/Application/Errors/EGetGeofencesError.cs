namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when the geofences of a ranch are listed.
/// </summary>
public enum EGetGeofencesError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden
}
