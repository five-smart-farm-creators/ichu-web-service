namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

/// <summary>
/// Reports a rule or access failure using a stable code that can be mapped to an API response.
/// </summary>
public sealed class CattleInformationException(
    string code,
    string message,
    Guid? conflictingCattleId = null) : Exception(message)
{
    public string Code { get; } = code;
    /// <summary>
    /// Gets the animal occupying a duplicate ear tag when its identity is available.
    /// </summary>
    public Guid? ConflictingCattleId { get; } = conflictingCattleId;
}
