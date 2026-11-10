namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record RecordWithdrawalPeriodResource(
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
