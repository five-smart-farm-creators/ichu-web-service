using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record UpdateCattleCommand(Guid RanchId, Guid CattleId, string EarTag, Breed Breed, Sex Sex, DateOnly BirthDate, Guid ExpectedVersion);
