namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
/// Requests removal of membership using the lot's expected version while retaining the animal.
/// </summary>
public sealed record RemoveCattleFromLotCommand(Guid RanchId, Guid LotId, Guid CattleId, Guid ExpectedVersion);
