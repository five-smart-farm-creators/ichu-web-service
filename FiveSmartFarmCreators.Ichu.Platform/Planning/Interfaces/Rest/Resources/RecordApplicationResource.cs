namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record RecordApplicationResource(
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
