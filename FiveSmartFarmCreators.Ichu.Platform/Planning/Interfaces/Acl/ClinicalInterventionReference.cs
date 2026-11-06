namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Interfaces.Acl;

/// <summary>
/// Carries the intervention identifiers needed to link a Planning record to OperationsMonitoring.
/// </summary>
/// <param name="InterventionId">The clinical intervention identifier.</param>
/// <param name="CattleId">The animal identifier associated with the intervention.</param>
public sealed record ClinicalInterventionReference(Guid InterventionId, Guid CattleId);
