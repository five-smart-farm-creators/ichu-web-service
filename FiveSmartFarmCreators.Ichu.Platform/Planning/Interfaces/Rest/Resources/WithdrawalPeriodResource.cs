namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

public sealed record WithdrawalPeriodResource(
    Guid Id,
    Guid RanchId,
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
