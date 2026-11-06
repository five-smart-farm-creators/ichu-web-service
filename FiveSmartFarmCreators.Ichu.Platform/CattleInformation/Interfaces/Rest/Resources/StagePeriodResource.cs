namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

/// <summary>
/// Exposes an inclusive stage start and exclusive end; null marks an open period.
/// </summary>
public sealed record StagePeriodResource(Guid Id, string LifeStage, DateOnly ValidFrom, DateOnly? ValidTo);
