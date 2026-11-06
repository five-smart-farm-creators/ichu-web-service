namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Acl;

/// <summary>Immutable identity and lifecycle facts; all dates are calendar dates, not timestamps.</summary>
public sealed record CattleSnapshot(
    Guid Id,
    Guid RanchId,
    string EarTag,
    string Status,
    string LifeStage,
    DateOnly BirthDate,
    string? ExitReason,
    DateOnly? ExitDate,
    Guid Version);
