namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Exposes animal details, retained lifecycle history and the version required for subsequent changes.
/// </summary>
public sealed record CattleResource(
    Guid Id,
    Guid RanchId,
    string EarTag,
    string Breed,
    string Sex,
    DateOnly BirthDate,
    string CurrentStage,
    string Status,
    string? ExitReason,
    DateOnly? ExitDate,
    Guid Version,
    IReadOnlyList<StagePeriodResource> StagePeriods);
