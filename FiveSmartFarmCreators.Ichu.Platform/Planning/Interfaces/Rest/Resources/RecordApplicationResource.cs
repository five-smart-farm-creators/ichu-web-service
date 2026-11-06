namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Request body for recording an application against a planned activity.
/// </summary>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="AppliedAt">The UTC application instant in ISO 8601 format.</param>
/// <param name="InterventionId">An optional OperationsMonitoring intervention identifier.</param>
public sealed record RecordApplicationResource(
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
