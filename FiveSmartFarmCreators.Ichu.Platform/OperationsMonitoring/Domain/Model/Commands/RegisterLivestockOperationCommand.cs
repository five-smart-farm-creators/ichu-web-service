namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
/// A field event or clinical intervention. Required values are nullable so that every missing field can be
/// reported together, as US-21 and US-23 require.
/// </summary>
public sealed record RegisterLivestockOperationCommand(
    Guid OperationId,
    Guid? AnimalId,
    string? Type,
    DateTimeOffset? OccurredAt,
    string? Observation,
    string? Product,
    string? Dose);
