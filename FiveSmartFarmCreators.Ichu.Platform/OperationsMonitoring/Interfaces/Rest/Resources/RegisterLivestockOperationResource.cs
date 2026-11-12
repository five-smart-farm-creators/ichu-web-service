namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The client generates the operation identifier once and reuses it on every retry. The reproductive fields are optional and apply only to the operation types that define them.
/// </summary>
public record RegisterLivestockOperationResource(
    Guid OperationId,
    Guid? CattleId,
    string? Type,
    DateTimeOffset? OccurredAt,
    string? Observation,
    string? Product,
    string? Dose,
    string? SireReference,
    string? PregnancyResult,
    DateOnly? ExpectedCalvingDate);
