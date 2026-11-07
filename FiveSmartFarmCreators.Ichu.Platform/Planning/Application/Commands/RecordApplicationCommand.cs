namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Commands;

/// <summary>
/// Requests recording that a planned activity was applied to an animal.
/// </summary>
/// <param name="ActivityId">The planned activity identifier.</param>
/// <param name="CattleId">The animal identifier.</param>
/// <param name="AppliedAt">The UTC instant when the application took place.</param>
/// <param name="InterventionId">An optional OperationsMonitoring intervention identifier.</param>
public sealed record RecordApplicationCommand(
    Guid ActivityId,
    Guid CattleId,
    DateTimeOffset AppliedAt,
    Guid? InterventionId);
