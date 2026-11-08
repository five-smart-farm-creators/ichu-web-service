using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.Commands;

/// <summary>
///     Requests a dated exit while retaining the animal's identity and distinguishing death from other exit reasons.
/// </summary>
public sealed record DeactivateCattleCommand(
    Guid RanchId,
    CattleId CattleId,
    ExitReason Reason,
    DateOnly ExitDate,
    Guid ExpectedVersion);
