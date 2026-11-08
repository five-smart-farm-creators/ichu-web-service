namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     The client generates the operation identifier once and reuses it on every retry.
/// </summary>
public record RegisterLivestockOperationResource(
    Guid OperationId,
    Guid? AnimalId,
    string? Type,
    DateTimeOffset? OccurredAt,
    string? Observation,
    string? Product,
    string? Dose);
