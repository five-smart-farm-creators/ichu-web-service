namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Acl;

/// <summary>
///     A clinical intervention applied to an animal, identified by the operation identifier that the client generated.
/// </summary>
public record ClinicalInterventionRecord(Guid OperationId, Guid RanchId, Guid CattleId, string Type, DateTimeOffset OccurredAt);
