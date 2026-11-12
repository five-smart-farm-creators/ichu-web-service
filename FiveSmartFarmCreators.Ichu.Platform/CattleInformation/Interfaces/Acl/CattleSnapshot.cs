namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>
/// Transfers immutable identity and lifecycle facts to other contexts; dates have calendar-day precision.
/// </summary>
public sealed record CattleSnapshot(
    Guid Id,
    Guid RanchId,
    string EarTag,
    string Status,
    string Sex,
    string LifeStage,
    DateOnly BirthDate,
    string? ExitReason,
    DateOnly? ExitDate,
    Guid Version);
