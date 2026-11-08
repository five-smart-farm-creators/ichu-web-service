using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Requests removal of membership using the lot's expected version while retaining the animal.
/// </summary>
public sealed record RemoveCattleFromLotCommand(Guid RanchId, LotId LotId, CattleId CattleId, Guid ExpectedVersion);
