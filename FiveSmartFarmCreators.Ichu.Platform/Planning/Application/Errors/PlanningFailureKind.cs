namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Errors;

/// <summary>
/// Classifies expected Planning use-case failures for the REST boundary.
/// </summary>
public enum PlanningFailureKind
{
    /// <summary>
    /// The requested Planning or external reference does not exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The request conflicts with an existing Planning relationship.
    /// </summary>
    Conflict,

    /// <summary>
    /// The current account is not allowed to access the requested ranch.
    /// </summary>
    Forbidden
}
