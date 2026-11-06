namespace FiveSmartFarmCreators.Ichu.Platform.CattleInformation.Interfaces.Rest.Resources;

public sealed record StagePeriodResource(Guid Id, string LifeStage, DateOnly ValidFrom, DateOnly? ValidTo);
