using FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Supplies an exit reason and calendar event date guarded by the animal's current version.
/// </summary>
public sealed record DeactivateCattleResource(ExitReason Reason, DateOnly ExitDate, Guid ExpectedVersion);
