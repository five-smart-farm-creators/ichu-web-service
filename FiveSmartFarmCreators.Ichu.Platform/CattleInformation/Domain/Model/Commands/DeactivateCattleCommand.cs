using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

public sealed record DeactivateCattleCommand(Guid RanchId, Guid CattleId, ExitReason Reason, DateOnly ExitDate, Guid ExpectedVersion);
