namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Response resource for an externally determined withdrawal period.
/// </summary>
/// <param name="Id">The withdrawal period identifier.</param>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="InterventionId">The optional OperationsMonitoring intervention identifier.</param>
/// <param name="StartsAt">The UTC start instant.</param>
/// <param name="EndsAt">The UTC end instant.</param>
public sealed record WithdrawalPeriodResource(
    Guid Id,
    Guid RanchId,
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
