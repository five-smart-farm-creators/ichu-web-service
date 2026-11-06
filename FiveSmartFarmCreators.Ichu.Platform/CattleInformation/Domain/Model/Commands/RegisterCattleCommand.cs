using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
/// Supplies animal identity and the stage known at registration; it does not establish earlier stage history.
/// </summary>
public sealed record RegisterCattleCommand(Guid RanchId, string EarTag, Breed Breed, Sex Sex, DateOnly BirthDate, LifeStage LifeStage);
