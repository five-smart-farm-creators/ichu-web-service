namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Rest.Resources;

/// <summary>
/// Response resource for an application record.
/// </summary>
/// <param name="Id">The application record identifier.</param>
/// <param name="ActivityId">The related planned activity identifier.</param>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="AppliedAt">The UTC application instant.</param>
/// <param name="InterventionId">The optional OperationsMonitoring intervention identifier.</param>
public sealed record ApplicationRecordResource(
    Guid Id,
    Guid ActivityId,
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
