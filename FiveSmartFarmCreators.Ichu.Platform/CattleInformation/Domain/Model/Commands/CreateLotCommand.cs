namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record CreateLotCommand(Guid RanchId, string Name, string? Criterion);
