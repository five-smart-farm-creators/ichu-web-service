using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record RegisterCattleCommand(Guid RanchId, string EarTag, Breed Breed, Sex Sex, DateOnly BirthDate, LifeStage LifeStage);
