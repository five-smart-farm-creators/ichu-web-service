using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Requests a same-ranch assignment using the lot's expected version, not the animal's version.
/// </summary>
public sealed record AssignCattleToLotCommand(Guid RanchId, LotId LotId, CattleId CattleId, Guid ExpectedVersion);
