namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
///     Lets other contexts count herd population, deaths and lot membership without loading animal aggregates.
/// </summary>
public sealed record HerdMemberSnapshot(Guid CattleId, Guid? LotId, DateOnly BirthDate, string? ExitReason, DateOnly? ExitDate);
