namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Errors;

public sealed class CattleInformationException(
    string code,
    string message,
    Guid? conflictingCattleId = null) : Exception(message)
{
    public string Code { get; } = code;
    public Guid? ConflictingCattleId { get; } = conflictingCattleId;
}
