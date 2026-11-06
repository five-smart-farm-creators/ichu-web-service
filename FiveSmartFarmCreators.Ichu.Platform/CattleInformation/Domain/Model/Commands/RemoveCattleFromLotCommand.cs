namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record RemoveCattleFromLotCommand(Guid RanchId, Guid LotId, Guid CattleId, Guid ExpectedVersion);
