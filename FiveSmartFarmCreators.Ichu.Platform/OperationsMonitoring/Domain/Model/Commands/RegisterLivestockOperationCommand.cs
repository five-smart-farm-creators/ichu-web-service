using FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.ValueObjects;

namespace FiveSmartFarmCreators.Ichu.Platform.OperationsMonitoring.Domain.Model.Commands;

/// <summary>
///     Registers a field event or a clinical intervention. Required values are null when the client omitted them or sent an unknown operation type, so that every missing field can be reported together.
///     The ranch is set for route-scoped registrations and null for mobile batch entries.
/// </summary>
public record RegisterLivestockOperationCommand(
    Guid? RanchId,
    Guid OperationId,
    Guid? AnimalId,
    OperationType? Type,
    DateTimeOffset? OccurredAt,
    string? Observation,
    string? Product,
    string? Dose);
