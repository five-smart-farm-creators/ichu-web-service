namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when the livestock operations of an animal are listed.
/// </summary>
public enum EGetLivestockOperationsError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The animal does not exist.</summary>
    AnimalNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden
}
