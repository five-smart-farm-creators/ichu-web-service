namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;

/// <summary>
/// Requests recording an externally determined withdrawal interval.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="InterventionId">An optional OperationsMonitoring intervention identifier.</param>
/// <param name="StartsAt">The explicit UTC start instant.</param>
/// <param name="EndsAt">The explicit UTC end instant.</param>
public sealed record RecordWithdrawalPeriodCommand(
    Guid RanchId,
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
