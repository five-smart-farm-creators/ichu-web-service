using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Proposes identity corrections guarded by the animal's version; stage and exit changes use separate commands.
/// </summary>
public sealed record UpdateCattleCommand(
    Guid RanchId,
    CattleId CattleId,
    string EarTag,
    Breed Breed,
    Sex Sex,
    DateOnly BirthDate,
    Guid ExpectedVersion);
