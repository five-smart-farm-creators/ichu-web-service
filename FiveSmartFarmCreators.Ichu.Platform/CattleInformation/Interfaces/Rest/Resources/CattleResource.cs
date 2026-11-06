namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

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
