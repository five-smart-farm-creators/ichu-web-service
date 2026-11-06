namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Application.Errors;

/// <summary>
///     Represents the failures that may occur when the latest indicators of an animal are retrieved.
/// </summary>
public enum EGetLatestAnimalTelemetryError
{
    /// <summary>No verified account is available for the request.</summary>
    Unauthenticated,

    /// <summary>The animal does not exist.</summary>
    AnimalNotFound,

    /// <summary>The account lacks the required permission on the ranch.</summary>
    Forbidden
}
