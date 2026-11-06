namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Request body for recording an externally determined withdrawal period.
/// </summary>
/// <param name="RanchId">The ranch identifier.</param>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="InterventionId">An optional OperationsMonitoring intervention identifier.</param>
/// <param name="StartsAt">The explicit UTC start instant in ISO 8601 format.</param>
/// <param name="EndsAt">The explicit UTC end instant in ISO 8601 format.</param>
public sealed record RecordWithdrawalPeriodResource(
    Guid RanchId,
    Guid CattleId,
    Guid? InterventionId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
