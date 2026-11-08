namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record ApplicationRecordResource(
    Guid Id,
    Guid ActivityId,
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
