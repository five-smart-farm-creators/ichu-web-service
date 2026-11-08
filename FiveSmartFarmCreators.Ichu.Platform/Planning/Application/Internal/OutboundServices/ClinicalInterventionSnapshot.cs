namespace FiveSmartFarmCreators.Ichu.Platform.Planning.Application.Internal.OutboundServices;

/// <summary>
/// The identifiers of a clinical intervention that Planning needs to check an application or withdrawal period.
/// </summary>
public sealed record ClinicalInterventionSnapshot(Guid InterventionId, Guid RanchId, Guid CattleId);
