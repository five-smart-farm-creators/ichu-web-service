namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Domain.Model.Commands;

public sealed record RecordWithdrawalPeriodCommand(
    Guid RanchId,
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
